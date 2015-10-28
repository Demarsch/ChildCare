using System;
using Core;
using System.Linq;
using System.Collections.ObjectModel;
using Core.Wpf.Mvvm;
using Prism.Mvvm;
using PatientRecordsModule.Services;
using PatientRecordsModule.DTO;
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

namespace PatientRecordsModule.ViewModels
{
    public class PersonHierarchicalAssignmentsViewModel : BindableBase
    {
        #region Fields
        private readonly AssignmentDTO assignment;

        private readonly IPatientRecordsService patientRecordsService;

        private readonly ILog logService;

        private readonly CommandWrapper reloadPatientVisitsCommandWrapper;

        private CancellationTokenSource currentLoadingToken;
        #endregion

        #region Constructors
        public PersonHierarchicalAssignmentsViewModel(AssignmentDTO assignment, IPatientRecordsService patientRecordsService, ILog logService)
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
            this.logService = logService;
            this.patientRecordsService = patientRecordsService;
            this.assignment = assignment;
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
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

        public CriticalFailureMediator CriticalFailureMediator { get; private set; }

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
            logService.InfoFormat("Loading child items for assignment with Id {0}...", assignment.Id);
            IDisposableQueryable<Assignment> childAssignmentsQuery = null;
            try
            {
                childAssignmentsQuery = patientRecordsService.GetAssignmentsChildAssignmentsQuery(assignment.Id);
                var loadChildAssignmentsTask = childAssignmentsQuery.Select(x => new AssignmentDTO()
                {
                    Id = x.Id,
                    ActualDateTime = x.AssignDateTime,
                    FinancingSourceName = x.FinancingSource.ShortName,
                    RecordTypeName = x.RecordType.Name,
                    RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name,
                }).ToListAsync(token);
                await Task.WhenAll(loadChildAssignmentsTask, Task.Delay(AppConfiguration.PendingOperationDelay, token));
                NestedItems.AddRange(loadChildAssignmentsTask.Result.Select(x => new PersonHierarchicalAssignmentsViewModel(x, patientRecordsService, logService)));
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load child items for assignment with Id {0}", assignment.Id);
                CriticalFailureMediator.Activate("Не удалость загрузить вложенные элементы назначения. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientVisitsCommandWrapper, ex);
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
            }
        }
        #endregion
    }
}
