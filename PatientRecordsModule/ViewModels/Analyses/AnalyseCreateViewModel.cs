using Core.Data;
using Core.Data.Services;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using Core.Extensions;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using Shared.PatientRecords.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Core.Data.Misc;
using Core.Data.Classes;

namespace Shared.PatientRecords.ViewModels
{
    public class AnalyseCreateViewModel : BindableBase, IDisposable, IDialogViewModel
    {
        private readonly IPatientRecordsService recordService;
        private readonly ILog logService;
        private readonly IDialogServiceAsync dialogService;
        private readonly IDialogService messageService;
        private readonly IUserService userService;
        private CancellationTokenSource currentLoadingToken;
        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; set; }
        private readonly CommandWrapper reloadDataSourceCommandWrapper;
        private readonly CommandWrapper reloadParametersCommandWrapper;
        private int personId;
        private int assignmentId;
        private int recordId;
        private int visitId;

        public AnalyseCreateViewModel(IPatientRecordsService recordService, IDialogServiceAsync dialogService, IDialogService messageService, ILog logService, IUserService userService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            } 
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            this.recordService = recordService;
            this.logService = logService;
            this.dialogService = dialogService;
            this.messageService = messageService;
            this.userService = userService;

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();

            CloseCommand = new DelegateCommand<bool?>(Close);
            reloadDataSourceCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => Initialize(personId, assignmentId, recordId, visitId)), CommandName = "Повторить" };
            reloadParametersCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => LoadParameters(SelectedAnalyseId)), CommandName = "Повторить" };

            FinSources = new ObservableCollectionEx<FieldValue>();
            Urgentlies = new ObservableCollectionEx<FieldValue>();
            ExecutionPlaces = new ObservableCollectionEx<FieldValue>();
            Visits = new ObservableCollectionEx<FieldValue>();
            Analyses = new ObservableCollectionEx<FieldValue>();
            Parameters = new ObservableCollectionEx<AnalyseParameterViewModel>();
        }

        internal async void Initialize(int personId, int assignmentId, int recordId, int visitId)
        {
            this.personId = personId;
            this.assignmentId = assignmentId;
            this.recordId = recordId;
            this.visitId = visitId;            
            FinSources.Clear();
            Visits.Clear();
            ExecutionPlaces.Clear();
            Urgentlies.Clear();
            Analyses.Clear();
            BusyMediator.Activate("Загрузка данных...");
            logService.Info("Loading data sources for create analyse...");
            IDisposableQueryable<Visit> visitQuery = null;
            IDisposableQueryable<RecordType> recordTypesQuery = null;
            IDisposableQueryable<FinancingSource> finSourcesQuery = null;
            IDisposableQueryable<Urgently> urgentliesQuery = null;
            IDisposableQueryable<ExecutionPlace> executionPlacesQuery = null;
            try
            {
                bool allowAssignToClosedVisits = userService.HasPermission(PermissionType.ChangeRecordParentVisit);
                visitQuery = recordService.GetPersonVisitsQuery(this.personId, !allowAssignToClosedVisits);
                var visitsSelectQuery = await visitQuery.Select(x => new { x.Id, x.BeginDateTime, x.VisitTemplate.Name }).ToArrayAsync();
                Visits.Add(new FieldValue() { Value = SpecialValues.NonExistingId, Field = "Предварительная запись" });
                Visits.AddRange(visitsSelectQuery.Select(x => new FieldValue() { Value = x.Id, Field = x.BeginDateTime.ToString("dd.MM.yyyy HH:mm") + " - " + x.Name }));

                recordTypesQuery = recordService.GetRecordTypes(true);
                var recordTypesSelectQuery = await recordTypesQuery.Select(x => new { x.Id, x.Name }).ToArrayAsync();
                Analyses.Add(new FieldValue() { Value = SpecialValues.NonExistingId, Field = "- выберите исследование -" });
                Analyses.AddRange(recordTypesSelectQuery.Select(x => new FieldValue() { Value = x.Id, Field = x.Name }));
                SelectedAnalyseId = SpecialValues.NonExistingId;

                finSourcesQuery = recordService.GetActualFinancingSources();
                var finSourcesSelectQuery = await finSourcesQuery.Select(x => new { x.Id, x.Name }).ToArrayAsync();
                FinSources.Add(new FieldValue() { Value = SpecialValues.NonExistingId, Field = "- выберите ист. финансирования -" });
                FinSources.AddRange(finSourcesSelectQuery.Select(x => new FieldValue() { Value = x.Id, Field = x.Name }));
               
                urgentliesQuery = recordService.GetActualUrgentlies(DateTime.Now);
                var urgentliesSelectQuery = await urgentliesQuery.Select(x => new { x.Id, x.Name }).ToArrayAsync();
                Urgentlies.Add(new FieldValue() { Value = SpecialValues.NonExistingId, Field = "- выберите форму оказания мед. помощи -" });
                Urgentlies.AddRange(urgentliesSelectQuery.Select(x => new FieldValue() { Value = x.Id, Field = x.Name }));

                executionPlacesQuery = recordService.GetActualExecutionPlaces();
                var executionPlacesSelectQuery = await executionPlacesQuery.Select(x => new { x.Id, x.Name }).ToArrayAsync();
                ExecutionPlaces.Add(new FieldValue() { Value = SpecialValues.NonExistingId, Field = "- выберите место выполнения -" });
                ExecutionPlaces.AddRange(executionPlacesSelectQuery.Select(x => new FieldValue() { Value = x.Id, Field = x.Name }));

                
                if (SpecialValues.IsNewOrNonExisting(this.visitId))
                {
                    if (visitQuery.Any(x => x.IsCompleted != true))
                        this.visitId = visitQuery.Where(x => x.IsCompleted != true).OrderByDescending(x => x.BeginDateTime).First().Id;
                    if (!SpecialValues.IsNewOrNonExisting(this.recordId))
                    {
                        var record = recordService.GetRecord(this.recordId).First();
                        if (record.Visit.IsCompleted != true || allowAssignToClosedVisits)
                            this.visitId = record.VisitId;
                    }
                    else if (!SpecialValues.IsNewOrNonExisting(this.assignmentId))
                    {
                        var assignment = recordService.GetAssignment(this.assignmentId).First();
                        if (assignment.VisitId.HasValue && (assignment.Visit.IsCompleted != true || allowAssignToClosedVisits))
                            this.visitId = assignment.VisitId.Value;
                    }
                }

                if (!SpecialValues.IsNewOrNonExisting(this.visitId))
                {
                    var visit = recordService.GetVisit(this.visitId).First();
                    SelectedVisitId = this.visitId;
                    SelectedFinSourceId = visit.FinancingSourceId;
                    SelectedUrgentlyId = visit.UrgentlyId;
                    SelectedExecutionPlaceId = visit.ExecutionPlaceId;
                }
                else
                {
                    SelectedVisitId = SpecialValues.NonExistingId;
                    SelectedFinSourceId = SpecialValues.NonExistingId;
                    SelectedUrgentlyId = SpecialValues.NonExistingId;
                    SelectedExecutionPlaceId = SpecialValues.NonExistingId;
                }
                AssignDateTime = DateTime.Now;
                logService.InfoFormat("Data sources for create analyse are successfully loaded");
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources for create analyse");
                FailureMediator.Activate("Не удалось загрузить данные для назначения анализов. Попробуйте еще раз или обратитесь в службу поддержки", reloadDataSourceCommandWrapper, ex);
            }
            finally
            {
                if (visitQuery != null)
                    visitQuery.Dispose();
                if (recordTypesQuery != null)
                    recordTypesQuery.Dispose();
                if (finSourcesQuery != null)
                    finSourcesQuery.Dispose();
                if (urgentliesQuery != null)
                    urgentliesQuery.Dispose();
                if (executionPlacesQuery != null)
                    executionPlacesQuery.Dispose();  
                BusyMediator.Deactivate();
            }
        }

        private async void LoadParameters(int recordTypeId)
        {
            Parameters.Clear();
            BusyMediator.Activate("Загрузка параметров...");
            logService.Info("Loading analyse parameters ...");
            IDisposableQueryable<RecordType> parametersQuery = null;
            try
            {
                parametersQuery = recordService.GetChildRecordTypesQuery(SelectedAnalyseId);
                var parametersSelectQuery = await parametersQuery.Select(x => new { x.Id, x.Name, x.ShortName }).ToArrayAsync();
                Parameters.AddRange(parametersSelectQuery.Select(x => new AnalyseParameterViewModel() { Id = x.Id, Name = x.Name, ShortName = x.ShortName }));
                CheckAllParameters(true);
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load analyse parameters");
                FailureMediator.Activate("Не удалось загрузить параметры выбранного анализа. Попробуйте еще раз или обратитесь в службу поддержки", reloadParametersCommandWrapper, ex);
            }
            finally
            {
                if (parametersQuery != null)
                    parametersQuery.Dispose();               
                BusyMediator.Deactivate();
            }
        }

        private void CheckAllParameters(bool isChecked)
        {
            Parameters.ForEach(x => x.IsChecked = isChecked);
        }
        
        #region Properties

        private bool isCheckAllParameters;
        public bool IsCheckAllParameters
        {
            get { return isCheckAllParameters; }
            set 
            { 
                SetProperty(ref isCheckAllParameters, value);
                CheckAllParameters(value);
            }
        }
              
        private ObservableCollectionEx<AnalyseParameterViewModel> parameters;
        public ObservableCollectionEx<AnalyseParameterViewModel> Parameters
        {
            get { return parameters; }
            set { SetProperty(ref parameters, value); }
        }

        private AnalyseParameterViewModel selectedParameter;
        public AnalyseParameterViewModel SelectedParameter
        {
            get { return selectedParameter; }
            set { SetProperty(ref selectedParameter, value); }
        }

        private ObservableCollectionEx<FieldValue> visits;
        public ObservableCollectionEx<FieldValue> Visits
        {
            get { return visits; }
            set { SetProperty(ref visits, value); }
        }

        private int selectedVisitId;
        public int SelectedVisitId
        {
            get { return selectedVisitId; }
            set { SetProperty(ref selectedVisitId, value); }
        }

        private ObservableCollectionEx<FieldValue> analyses;
        public ObservableCollectionEx<FieldValue> Analyses
        {
            get { return analyses; }
            set { SetProperty(ref analyses, value); }
        }

        private int selectedAnalyseId;
        public int SelectedAnalyseId
        {
            get { return selectedAnalyseId; }
            set 
            {
                if (SetProperty(ref selectedAnalyseId, value))
                    LoadParameters(value);
            }
        }       

        private ObservableCollectionEx<FieldValue> finSources;
        public ObservableCollectionEx<FieldValue> FinSources
        {
            get { return finSources; }
            set { SetProperty(ref finSources, value); }
        }

        private int selectedFinSourceId;
        public int SelectedFinSourceId
        {
            get { return selectedFinSourceId; }
            set { SetProperty(ref selectedFinSourceId, value); }
        }

        private ObservableCollectionEx<FieldValue> urgentlies;
        public ObservableCollectionEx<FieldValue> Urgentlies
        {
            get { return urgentlies; }
            set { SetProperty(ref urgentlies, value); }
        }

        private int selectedUrgentlyId;
        public int SelectedUrgentlyId
        {
            get { return selectedUrgentlyId; }
            set { SetProperty(ref selectedUrgentlyId, value); }
        }

        private ObservableCollectionEx<FieldValue> executionPlaces;
        public ObservableCollectionEx<FieldValue> ExecutionPlaces
        {
            get { return executionPlaces; }
            set { SetProperty(ref executionPlaces, value); }
        }

        private int selectedExecutionPlaceId;
        public int SelectedExecutionPlaceId
        {
            get { return selectedExecutionPlaceId; }
            set { SetProperty(ref selectedExecutionPlaceId, value); }
        }

        private DateTime assignDateTime;
        public DateTime AssignDateTime
        {
            get { return assignDateTime; }
            set { SetProperty(ref assignDateTime, value); }
        }

        #endregion

        #region IDialogViewModel

        public string Title
        {
            get { return "Запись на лабораторные исследования"; }
        }

        public string ConfirmButtonText
        {
            get { return "Записать"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        private void Close(bool? validate)
        {
            if (validate == true)
            {
                //return;                
            }
            else
            {
                OnCloseRequested(new ReturnEventArgs<bool>(false));
            }
        }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(ReturnEventArgs<bool> e)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion               
    
        public void Dispose()
        {

        }
    }
}
