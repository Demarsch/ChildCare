using System;
using Core;
using System.Linq;
using System.Collections.ObjectModel;
using Core.Wpf.Mvvm;
using Prism.Mvvm;
using Shared.PatientRecords.Services;
using Shared.PatientRecords.DTO;
using log4net;
using Core.Wpf.Misc;
using System.Threading;
using Prism.Commands;
using Core.Data.Misc;
using Core.Data;
using Core.Extensions;
using System.Data.Entity;
using System.Threading.Tasks;
using Core.Misc;
using System.Windows.Input;
using Prism.Events;
using Core.Wpf.Events;
using System.Collections.Generic;
using Shared.PatientRecords.Misc;

namespace Shared.PatientRecords.ViewModels
{
    public class PersonHierarchicalAssignmentsViewModel : BindableBase, IDisposable, IHierarchicalItem
    {
        #region Fields
        private readonly AssignmentDTO assignment;

        private readonly IPatientRecordsService patientRecordsService;

        private readonly IEventAggregator eventAggregator;

        private readonly ILog logService;

        private readonly CommandWrapper reloadPatientVisitsCommandWrapper;

        private CancellationTokenSource currentLoadingToken;
        #endregion

        #region Constructors
        public PersonHierarchicalAssignmentsViewModel(AssignmentDTO assignment, IPatientRecordsService patientRecordsService, IEventAggregator eventAggregator, ILog logService)
        {
            if (assignment == null)
            {
                throw new ArgumentNullException("assignment");
            }
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            this.eventAggregator = eventAggregator;
            this.logService = logService;
            this.patientRecordsService = patientRecordsService;
            this.assignment = assignment;
            this.Item = new PersonItem() { Id = assignment.Id, Type = ItemType.Assignment };
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            reloadPatientVisitsCommandWrapper = new CommandWrapper
            {
                Command = new DelegateCommand(() => LoadItemsAsync()),
                CommandName = "Повторить",
            };
        }
        #endregion

        #region Properties
        public int Id { get { return assignment.Id; } }

        public string AssignDateTime { get { return assignment.ActualDateTime.ToString("dd.MM.yyyy HH:mm"); } }

        public string RecordTypeName { get { return assignment.RecordTypeName; } }

        public string RoomName { get { return assignment.RoomName; } }

        public string FinSource { get { return assignment.FinancingSourceName; } }

        public DateTime ActualDateTime { get { return assignment.ActualDateTime; } }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (SetProperty(ref isSelected, value) && value)
                    eventAggregator.GetEvent<SelectionEvent<Assignment>>().Publish(this.Id);
            }
        }

        private PersonItem item;
        public PersonItem Item
        {
            get { return item; }
            set { SetProperty(ref item, value); }
        }

        private ObservableCollectionEx<IHierarchicalItem> childs;
        public ObservableCollectionEx<IHierarchicalItem> Childs
        {
            get
            {
                if (childs == null)
                    LoadItemsAsync();
                return childs;
            }
            set { SetProperty(ref childs, value); }
        }

        public FailureMediator FailureMediator { get; private set; }

        public BusyMediator BusyMediator { get; set; }
        #endregion

        #region Methods

        public void Dispose()
        {
            reloadPatientVisitsCommandWrapper.Dispose();
        }

        private async void LoadItemsAsync()
        {
            var loadingIsCompleted = false;
            if (childs == null)
                Childs = new ObservableCollectionEx<IHierarchicalItem>();
            Childs.Clear();
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate(string.Empty);
            logService.InfoFormat("Loading child items for assignment with Id {0}...", assignment.Id);
            IDisposableQueryable<Assignment> childAssignmentsQuery = null;
            IDisposableQueryable<Record> childRecordsQuery = null;
            try
            {
                childAssignmentsQuery = patientRecordsService.GetAssignmentsChildAssignmentsQuery(assignment.Id);
                childRecordsQuery = patientRecordsService.GetAssignmentsChildRecordsQuery(assignment.Id);
                var loadChildAssignmentsTask = childAssignmentsQuery.Select(x => new AssignmentDTO()
                {
                    Id = x.Id,
                    ActualDateTime = x.AssignDateTime,
                    FinancingSourceName = x.FinancingSource.Name,
                    RecordTypeName = x.RecordType.ShortName != string.Empty ? x.RecordType.ShortName : x.RecordType.Name,
                    RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name,
                }).ToListAsync(token);
                var loadChildRecordsTask = childRecordsQuery.Select(x => new RecordDTO()
                {
                    Id = x.Id,
                    ActualDateTime = x.ActualDateTime,
                    RecordTypeName = x.RecordType.Name,
                    BeginDateTime = x.BeginDateTime,
                    EndDateTime = x.EndDateTime,
                    IsCompleted = x.IsCompleted,
                    FinSourceName = "ист. фин.",
                    RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name,
                }).ToListAsync(token);
                await Task.WhenAll(loadChildAssignmentsTask, loadChildRecordsTask);
                var resChilds = new List<IHierarchicalItem>();
                resChilds.AddRange(loadChildAssignmentsTask.Result.Select(x => new PersonHierarchicalAssignmentsViewModel(x, patientRecordsService, eventAggregator, logService)));
                resChilds.AddRange(loadChildRecordsTask.Result.Select(x => new PersonHierarchicalRecordsViewModel(x, patientRecordsService, eventAggregator, logService)));
                Childs.AddRange(resChilds.OrderBy(x => x.ActualDateTime));
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load child items for assignment with Id {0}", assignment.Id);
                FailureMediator.Activate("Не удалость загрузить вложенные элементы назначения. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientVisitsCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (childAssignmentsQuery != null)
                {
                    childAssignmentsQuery.Dispose();
                }
                if (childRecordsQuery != null)
                {
                    childRecordsQuery.Dispose();
                }
            }
        }
        #endregion
    }
}
