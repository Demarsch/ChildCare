using System;
using Core;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using Shared.PatientRecords.Services;
using Core.Wpf.Mvvm;
using Shared.PatientRecords.DTO;
using log4net;
using Core.Wpf.Misc;
using System.Threading;
using Prism.Commands;
using Core.Data.Misc;
using Core.Data;
using System.Data.Entity;
using System.Threading.Tasks;
using Core.Misc;
using Core.Extensions;
using System.Windows.Input;
using Core.Wpf.Events;
using Prism.Events;

namespace Shared.PatientRecords.ViewModels
{
    public class PersonHierarchicalRecordsViewModel : BindableBase, IDisposable
    {
        #region Fields
        private readonly RecordDTO record;

        private readonly IPatientRecordsService patientRecordsService;

        private readonly IEventAggregator eventAggregator;

        private readonly ILog logService;

        private readonly CommandWrapper reloadPatientVisitsCommandWrapper;

        private CancellationTokenSource currentLoadingToken;
        #endregion

        #region Constructors
        public PersonHierarchicalRecordsViewModel(RecordDTO recordDTO, IPatientRecordsService patientRecordsService, IEventAggregator eventAggregator, ILog logService)
        {
            if (recordDTO == null)
            {
                throw new ArgumentNullException("recordDTO");
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
            this.record = recordDTO;
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
        public int Id { get { return record.Id; } }

        public string DateTimePeriod { get { return record.BeginDateTime.ToString("dd.MM.yyyy") + " - " + (record.EndDateTime.HasValue ? record.EndDateTime.Value.ToString("dd.MM.yyyy") : "..."); } }

        public string RecordTypeName { get { return record.RecordTypeName; } }

        public string FinSource { get { return record.FinSourceName; } }

        public string RoomName { get { return record.RoomName; } }

        public bool IsCompleted { get { return record.IsCompleted == true; } }

        private ObservableCollectionEx<object> nestedItems;
        public ObservableCollectionEx<object> NestedItems
        {
            get
            {
                if (nestedItems == null)
                    LoadItemsAsync();
                return nestedItems;
            }
            set { SetProperty(ref nestedItems, value); }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (SetProperty(ref isSelected, value) && value)
                    eventAggregator.GetEvent<SelectionEvent<Record>>().Publish(this.Id);
            }
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
            if (nestedItems == null)
                NestedItems = new ObservableCollectionEx<object>();
            NestedItems.Clear();
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate(string.Empty);
            logService.InfoFormat("Loading child items for record with Id {0}...", record.Id);
            IDisposableQueryable<Assignment> childAssignmentsQuery = null;
            IDisposableQueryable<Record> childRecordsQuery = null;
            try
            {
                childAssignmentsQuery = patientRecordsService.GetRecordsChildAssignmentsQuery(record.Id);
                childRecordsQuery = patientRecordsService.GetRecordsChildRecordsQuery(record.Id);
                var loadChildAssignmentsTask = childAssignmentsQuery.Select(x => new AssignmentDTO()
                {
                    Id = x.Id,
                    ActualDateTime = x.AssignDateTime,
                    FinancingSourceName = x.FinancingSource.Name,
                    RecordTypeName = x.RecordType.Name,
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
                NestedItems.AddRange(loadChildAssignmentsTask.Result.Select(x => new PersonHierarchicalAssignmentsViewModel(x, patientRecordsService, eventAggregator, logService)));
                NestedItems.AddRange(loadChildRecordsTask.Result.Select(x => new PersonHierarchicalRecordsViewModel(x, patientRecordsService, eventAggregator, logService)));
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to child items for record with Id {0}", record.Id);
                FailureMediator.Activate("Не удалость загрузить вложенные элементы услуги. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientVisitsCommandWrapper, ex);
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
