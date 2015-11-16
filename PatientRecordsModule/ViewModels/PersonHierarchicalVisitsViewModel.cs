using System;
using Core;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using PatientRecordsModule.Services;
using Core.Wpf.Mvvm;
using PatientRecordsModule.DTO;
using System.Threading.Tasks;
using System.Collections.Generic;
using log4net;
using Core.Extensions;
using Core.Wpf.Misc;
using Prism.Commands;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using System.Threading;
using System.Data.Entity;
using Core.Misc;
using Core.Wpf.Events;
using Prism.Events;

namespace PatientRecordsModule.ViewModels
{
    public class PersonHierarchicalVisitsViewModel : BindableBase
    {
        #region Fields
        private readonly VisitDTO visit;

        private readonly IPatientRecordsService patientRecordsService;

        private readonly IEventAggregator eventAggregator;

        private readonly ILog logService;


        private readonly CommandWrapper reloadPatientVisitsCommandWrapper;

        private CancellationTokenSource currentLoadingToken;
        #endregion

        #region Constructors
        public PersonHierarchicalVisitsViewModel(VisitDTO visitDTO, IPatientRecordsService patientRecordsService, IEventAggregator eventAggregator, ILog logService)
        {
            if (visitDTO == null)
            {
                throw new ArgumentNullException("visitDTO");
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
            this.visit = visitDTO;
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
        public int Id { get { return visit.Id; } }

        public string DateTimePeriod { get { return visit.BeginDateTime.ToString("dd.MM.yyyy") + " - " + (visit.EndDateTime.HasValue ? visit.EndDateTime.Value.ToString("dd.MM.yyyy") : "..."); } }

        public string Name { get { return visit.Name; } }

        public string FinSource { get { return visit.FinSource; } }

        public bool IsCompleted { get { return visit.IsCompleted == true; } }

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
                SetProperty(ref isSelected, value);
                eventAggregator.GetEvent<SelectionEvent<Visit>>().Publish(this.Id);
            }
        }

        public FailureMediator FailureMediator { get; private set; }

        public BusyMediator BusyMediator { get; set; }

        #endregion

        #region Methods
        private async void LoadItemsAsync()
        {
            var loadingIsCompleted = false;
            if (nestedItems == null)
                NestedItems = new ObservableCollectionEx<object>();
            NestedItems.Clear();
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate(string.Empty);
            logService.InfoFormat("Loading child items for visit with Id {0}...", visit.Id);
            IDisposableQueryable<Assignment> childAssignmentsQuery = null;
            IDisposableQueryable<Record> childRecordsQuery = null;
            try
            {
                childAssignmentsQuery = patientRecordsService.GetVisitsChildAssignmentsQuery(visit.Id);
                childRecordsQuery = patientRecordsService.GetVisitsChildRecordsQuery(visit.Id);
                var loadChildAssignmentsTask = childAssignmentsQuery.Select(x => new AssignmentDTO()
                    {
                        Id = x.Id,
                        ActualDateTime = x.AssignDateTime,
                        FinancingSourceName = x.FinancingSource.ShortName,
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
                logService.ErrorFormatEx(ex, "Failed to load child items for visit with Id {0}", visit.Id);
                FailureMediator.Activate("Не удалость загрузить вложенные элементы случая. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientVisitsCommandWrapper, ex);
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
