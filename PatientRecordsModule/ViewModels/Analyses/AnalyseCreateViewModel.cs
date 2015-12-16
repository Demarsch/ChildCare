using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Navigation;
using Core.Data;
using Core.Data.Classes;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using Shared.PatientRecords.Services;

namespace Shared.PatientRecords.ViewModels
{
    public class AnalyseCreateViewModel : BindableBase, IDisposable, IDialogViewModel, IDataErrorInfo
    {
        private readonly IPatientRecordsService recordService;

        private readonly ILog logService;

        private readonly IDialogServiceAsync dialogService;

        private readonly IDialogService messageService;

        private readonly IUserService userService;

        private readonly ISecurityService securityService;

        private CancellationTokenSource currentSavingToken;

        public BusyMediator BusyMediator { get; set; }

        private int personId;

        private int assignmentId;

        private int recordId;

        private int visitId;

        public AnalyseCreateViewModel(IPatientRecordsService recordService,
                                      IDialogServiceAsync dialogService,
                                      IDialogService messageService,
                                      ILog logService,
                                      IUserService userService,
                                      ISecurityService securityService)
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
            if (securityService == null)
            {
                throw new ArgumentNullException("securityService");
            }
            this.recordService = recordService;
            this.logService = logService;
            this.dialogService = dialogService;
            this.messageService = messageService;
            this.userService = userService;
            this.securityService = securityService;

            BusyMediator = new BusyMediator();

            CloseCommand = new DelegateCommand<bool?>(Close);

            FinSources = new ObservableCollectionEx<FieldValue>();
            Urgentlies = new ObservableCollectionEx<FieldValue>();
            ExecutionPlaces = new ObservableCollectionEx<FieldValue>();
            Visits = new ObservableCollectionEx<FieldValue>();
            Analyses = new ObservableCollectionEx<FieldValue>();
            Parameters = new ObservableCollectionEx<AnalyseParameterViewModel>();
            AssignedAnalyses = new List<KeyValuePair<int, int?>>();
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
            AssignedAnalyses.Clear();
            BusyMediator.Activate("Загрузка данных...");
            logService.Info("Loading data sources for create analyse...");
            IDisposableQueryable<Visit> visitQuery = null;
            IDisposableQueryable<RecordType> recordTypesQuery = null;
            IDisposableQueryable<FinancingSource> finSourcesQuery = null;
            IDisposableQueryable<Urgently> urgentliesQuery = null;
            IDisposableQueryable<ExecutionPlace> executionPlacesQuery = null;
            try
            {
                bool allowAssignToClosedVisits = securityService.HasPermission(Permission.ChangeRecordParentVisit);
                visitQuery = recordService.GetPersonVisitsQuery(this.personId, !allowAssignToClosedVisits);
                var visitsSelectQuery = await visitQuery.Select(x => new { x.Id, x.BeginDateTime, x.VisitTemplate.Name }).ToArrayAsync();
                Visits.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "Предварительная запись" });
                Visits.AddRange(visitsSelectQuery.Select(x => new FieldValue { Value = x.Id, Field = x.BeginDateTime.ToString("dd.MM.yyyy HH:mm") + " - " + x.Name }));

                recordTypesQuery = recordService.GetRecordTypes(true);
                var recordTypesSelectQuery = await recordTypesQuery.Select(x => new { x.Id, x.Name }).ToArrayAsync();
                Analyses.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- выберите исследование -" });
                Analyses.AddRange(recordTypesSelectQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
                SelectedAnalyseId = SpecialValues.NonExistingId;

                finSourcesQuery = recordService.GetActualFinancingSources();
                var finSourcesSelectQuery = await finSourcesQuery.Select(x => new { x.Id, x.Name }).ToArrayAsync();
                FinSources.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- выберите ист. финансирования -" });
                FinSources.AddRange(finSourcesSelectQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));

                urgentliesQuery = recordService.GetActualUrgentlies(DateTime.Now);
                var urgentliesSelectQuery = await urgentliesQuery.Select(x => new { x.Id, x.Name }).ToArrayAsync();
                Urgentlies.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- выберите форму оказания мед. помощи -" });
                Urgentlies.AddRange(urgentliesSelectQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));

                executionPlacesQuery = recordService.GetActualExecutionPlaces();
                var executionPlacesSelectQuery = await executionPlacesQuery.Select(x => new { x.Id, x.Name }).ToArrayAsync();
                ExecutionPlaces.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- выберите место выполнения -" });
                ExecutionPlaces.AddRange(executionPlacesSelectQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));


                if (this.visitId.IsNewOrNonExisting())
                {
                    if (visitQuery.Any(x => x.IsCompleted != true))
                    {
                        this.visitId = visitQuery.Where(x => x.IsCompleted != true).OrderByDescending(x => x.BeginDateTime).First().Id;
                    }
                    if (!this.recordId.IsNewOrNonExisting())
                    {
                        var record = recordService.GetRecord(this.recordId).First();
                        if (record.Visit.IsCompleted != true || allowAssignToClosedVisits)
                        {
                            this.visitId = record.VisitId;
                        }
                    }
                    else if (!this.assignmentId.IsNewOrNonExisting())
                    {
                        var assignment = recordService.GetAssignment(this.assignmentId).First();
                        if (assignment.VisitId.HasValue && (assignment.Visit.IsCompleted != true || allowAssignToClosedVisits))
                        {
                            this.visitId = assignment.VisitId.Value;
                        }
                    }
                }

                if (!this.visitId.IsNewOrNonExisting())
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
                    AnalyseDetails = string.Empty;
                }
                AssignDateTime = DateTime.Now;
                logService.InfoFormat("Data sources for create analyse are successfully loaded");
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources for create analyse");
                messageService.ShowError("Не удалось загрузить данные для назначения анализов. ");
            }
            finally
            {
                if (visitQuery != null)
                {
                    visitQuery.Dispose();
                }
                if (recordTypesQuery != null)
                {
                    recordTypesQuery.Dispose();
                }
                if (finSourcesQuery != null)
                {
                    finSourcesQuery.Dispose();
                }
                if (urgentliesQuery != null)
                {
                    urgentliesQuery.Dispose();
                }
                if (executionPlacesQuery != null)
                {
                    executionPlacesQuery.Dispose();
                }
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
                Parameters.AddRange(parametersSelectQuery.Select(x => new AnalyseParameterViewModel { Id = x.Id, Name = x.Name, ShortName = x.ShortName }));
                IsCheckAllParameters = true;
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load analyse parameters");
                messageService.ShowError("Не удалось загрузить параметры выбранного анализа.");
            }
            finally
            {
                if (parametersQuery != null)
                {
                    parametersQuery.Dispose();
                }
                BusyMediator.Deactivate();
            }
        }

        private void LoadVisitRelatedData(int visitId)
        {
            if (!visitId.IsNewOrNonExisting())
            {
                var visit = recordService.GetVisit(visitId).First();
                SelectedVisitId = this.visitId;
                SelectedFinSourceId = visit.FinancingSourceId;
                SelectedUrgentlyId = visit.UrgentlyId;
                SelectedExecutionPlaceId = visit.ExecutionPlaceId;
            }
            else
            {
                SelectedVisitId = SpecialValues.NonExistingId;
                SelectedFinSourceId = SpecialValues.NonExistingId;
                var urg = recordService.GetActualUrgentlies(DateTime.Now);
                SelectedUrgentlyId = urg.Any(x => x.IsDefalut) ? urg.First(x => x.IsDefalut).Id : SpecialValues.NonExistingId;
                SelectedExecutionPlaceId = SpecialValues.NonExistingId;
                AnalyseDetails = string.Empty;
            }
        }

        private void CheckAllParameters(bool isChecked)
        {
            Parameters.ForEach(x => x.IsChecked = isChecked);
        }

        private async void CreateAnalyseAssignment()
        {
            BusyMediator.Activate("Запись на анализ...");
            logService.Info("Create analyse ...");
            if (currentSavingToken != null)
            {
                currentSavingToken.Cancel();
                currentSavingToken.Dispose();
            }
            currentSavingToken = new CancellationTokenSource();
            var token = currentSavingToken.Token;
            var saveSuccesfull = false;
            try
            {
                var analyseAssignment = new Assignment();
                analyseAssignment.ParentAssignmentId = null;
                analyseAssignment.ParentRecordId = null;
                analyseAssignment.RecordTypeId = SelectedAnalyseId;
                analyseAssignment.PersonId = personId;
                analyseAssignment.AssignDateTime = AssignDateTime;
                analyseAssignment.Duration = 0;
                analyseAssignment.AssignUserId = userService.GetCurrentUser().Id;
                analyseAssignment.AssignLpuId = null;
                analyseAssignment.RoomId = recordService.GetRooms(AssignDateTime).First(x => x.Options.Contains(OptionValues.LaboratoryRoom)).Id;
                analyseAssignment.FinancingSourceId = SelectedFinSourceId;
                analyseAssignment.UrgentlyId = SelectedUrgentlyId;
                analyseAssignment.ExecutionPlaceId = SelectedExecutionPlaceId;
                analyseAssignment.ParametersOptions = GetAssignParameters();
                analyseAssignment.CancelUserId = null;
                analyseAssignment.CancelDateTime = null;
                analyseAssignment.Note = AnalyseDetails.ToSafeString();
                analyseAssignment.RecordId = null;
                analyseAssignment.VisitId = !SelectedVisitId.IsNewOrNonExisting() ? SelectedVisitId : (int?)null;
                analyseAssignment.IsTemporary = false;
                analyseAssignment.CreationDateTime = DateTime.Now;
                analyseAssignment.BillingDateTime = null; // договор??
                analyseAssignment.Cost = 0.0;
                analyseAssignment.RemovedByUserId = null;

                var exception = string.Empty;
                var createdAnalyseId = await recordService.CreateAnalyseAssignmentAsync(analyseAssignment, token);
                AssignedAnalyses.Add(new KeyValuePair<int, int?>(createdAnalyseId, analyseAssignment.VisitId));
                saveSuccesfull = true;
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to create analyse for person with Id = {0}", this.personId);
                messageService.ShowError("Не удалось создать запись на лабораторное исследование.");
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {
                    messageService.ShowInformation("Назначение прошло успешно.");
                }
            }
        }

        public List<KeyValuePair<int, int?>> AssignedAnalyses { get; private set; }

        private string GetAssignParameters()
        {
            return Parameters.Where(x => x.IsChecked).Select(x => x.Id.ToString()).Aggregate((x, y) => x + "|" + y);
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
            set
            {
                if (SetProperty(ref selectedVisitId, value))
                {
                    LoadVisitRelatedData(value);
                }
            }
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
                {
                    LoadParameters(value);
                }
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

        private string analyseDetails;

        public string AnalyseDetails
        {
            get { return analyseDetails; }
            set { SetProperty(ref analyseDetails, value); }
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

        public bool AssignIsSuccessful = false;

        private void Close(bool? validate)
        {
            assignWasRequested = true;
            if (validate == true)
            {
                if (IsValid)
                {
                    CreateAnalyseAssignment();
                    AssignIsSuccessful = true;
                }
                else
                {
                    messageService.ShowWarning("Проверьте правильность заполнения полей.");
                }
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

        #region IDataError

        private bool assignWasRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        private bool IsValid
        {
            get
            {
                assignWasRequested = true;
                OnPropertyChanged(string.Empty);
                return invalidProperties.Count < 1;
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                if (!assignWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                if (columnName == "SelectedAnalyseId")
                {
                    result = selectedAnalyseId.IsNewOrNonExisting() ? "Укажите наименование исследования" : string.Empty;
                }
                if (columnName == "SelectedFinSourceId")
                {
                    result = selectedFinSourceId.IsNewOrNonExisting() ? "Укажите источник финансирования" : string.Empty;
                }
                if (columnName == "SelectedUrgentlyId")
                {
                    result = selectedUrgentlyId.IsNewOrNonExisting() ? "Укажите форму оказания мед. помощи" : string.Empty;
                }
                if (columnName == "SelectedExecutionPlaceId")
                {
                    result = selectedExecutionPlaceId.IsNewOrNonExisting() ? "Укажите место выполнения" : string.Empty;
                }
                if (columnName == "AssignDateTime")
                {
                    DateTime date;
                    result = DateTime.TryParse(AssignDateTime.ToString("dd.MM.yyyy HH:mm"), out date) ? "Укажите корректную дату/время записи" : string.Empty;
                }
                if (columnName == "Parameters")
                {
                    result = Parameters.All(x => !x.IsChecked) ? "Не выбраны параметры назначаемого анализа" : string.Empty;
                }

                if (string.IsNullOrEmpty(result))
                {
                    invalidProperties.Remove(columnName);
                }
                else
                {
                    invalidProperties.Add(columnName);
                }
                return result;
            }
        }

        #endregion
    }
}