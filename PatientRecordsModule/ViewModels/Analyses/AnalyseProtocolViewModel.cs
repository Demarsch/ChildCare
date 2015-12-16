using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Misc;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Services;
using log4net;
using Shared.PatientRecords.Misc;
using Shared.PatientRecords.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Wpf.Mvvm;
using Shared.PatientRecords.DTO;
using System.Threading;

namespace Shared.PatientRecords.ViewModels
{
    public class AnalyseProtocolViewModel : TrackableBindableBase, IRecordTypeProtocol, IChangeTrackerMediator
    {
        private readonly IPatientRecordsService recordService;
        private readonly IDialogService messageService;
        private readonly IUserService userService;
        private readonly ICacheService cacheService;
        private readonly ILog logService;
        private readonly IDialogServiceAsync dialogService;
        private int personId;
        private bool isMale = false;
        private int years = 0;
        private DateTime date = DateTime.Now;
        private readonly IChangeTracker currentInstanceChangeTracker;
        private readonly Func<AnalyseRefferenceCollectionViewModel> refferencesViewModelFactory;

        #region Constructors
        public AnalyseProtocolViewModel(IPatientRecordsService recordService, ILog logService, IDialogService messageService, IDialogServiceAsync dialogService, 
            IUserService userService, ICacheService cacheService, Func<AnalyseRefferenceCollectionViewModel> refferencesViewModelFactory)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (refferencesViewModelFactory == null)
            {
                throw new ArgumentNullException("refferencesViewModelFactory");
            }
            this.recordService = recordService;
            this.cacheService = cacheService;
            this.messageService = messageService;
            this.userService = userService;
            this.logService = logService;
            this.dialogService = dialogService;
            this.refferencesViewModelFactory = refferencesViewModelFactory;

            CurrentMode = ProtocolMode.View;

            AnalyseResults = new ObservableCollectionEx<AnalyseResultViewModel>();
            AnalyseResults.BeforeCollectionChanged += OnBeforeDiagnosCollectionChanged;
            AnalyseResultsView = new ObservableCollectionEx<AnalyseResultViewModel>();

            ChartData = new ObservableCollectionEx<ChartPoint>();
            RefMinData = new ObservableCollectionEx<ChartPoint>();
            RefMaxData = new ObservableCollectionEx<ChartPoint>();

            setAnalyseRefferencesCommand = new DelegateCommand(SetAnalyseRefferences);

            currentInstanceChangeTracker = new ChangeTrackerEx<AnalyseProtocolViewModel>(this);
            var changeTracker = new CompositeChangeTracker(currentInstanceChangeTracker);
            ChangeTracker = changeTracker;
        }
       
        #endregion

        private void OnBeforeDiagnosCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<AnalyseResultViewModel>())
                    (ChangeTracker as CompositeChangeTracker).AddTracker(newItem.ChangeTracker);
            }
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.Cast<AnalyseResultViewModel>())
                    (ChangeTracker as CompositeChangeTracker).RemoveTracker(oldItem.ChangeTracker);
            }
        }

        #region Properties

        public IChangeTracker ChangeTracker { get; set; }

        public ObservableCollectionEx<AnalyseResultViewModel> AnalyseResults { get; private set; }

        private AnalyseResultViewModel selectedAnalyseResult;
        public AnalyseResultViewModel SelectedAnalyseResult
        {
            get { return selectedAnalyseResult; }
            set { SetProperty(ref selectedAnalyseResult, value); }
        }

        public ObservableCollectionEx<AnalyseResultViewModel> AnalyseResultsView { get; private set; }

        private AnalyseResultViewModel selectedAnalyseResultView;
        public AnalyseResultViewModel SelectedAnalyseResultView
        {
            get { return selectedAnalyseResultView; }
            set 
            {
                if (SetProperty(ref selectedAnalyseResultView, value) && value != null)
                    DrawChart(value);
            }
        }        

        private string selectedParameter;
        public string SelectedParameter
        {
            get { return selectedParameter; }
            set { SetProperty(ref selectedParameter, value); }
        }

        private bool showChart;
        public bool ShowChart
        {
            get { return showChart; }
            set { SetProperty(ref showChart, value); }

        }
        public bool IsEditMode
        {
            get 
            {
                return CurrentMode == ProtocolMode.Edit;
            }
        }

        public bool IsViewMode
        {
            get
            {
                return CurrentMode == ProtocolMode.View;    
            }
        }

        public ObservableCollectionEx<ChartPoint> ChartData { get; private set; }
        public ObservableCollectionEx<ChartPoint> RefMinData { get; private set; }
        public ObservableCollectionEx<ChartPoint> RefMaxData { get; private set; }

        private readonly DelegateCommand setAnalyseRefferencesCommand;
        public ICommand SetAnalyseRefferencesCommand { get { return setAnalyseRefferencesCommand; } }

        #endregion
 
        #region IRecordTypeProtocol implementation
        private ProtocolMode currentMode;
        public ProtocolMode CurrentMode
        {
            get { return currentMode; }
            set
            {
                SetProperty(ref currentMode, value);
                OnPropertyChanged(() => IsEditMode);
                OnPropertyChanged(() => IsViewMode);
            }
        }

        public void PrintProtocol()
        {
            int i = 0;
        }

        public int SaveProtocol(int recordId, int visitId)
        {
            if (recordId == SpecialValues.NewId || !ProtocolIsValid())
                return SpecialValues.NonExistingId;
            
            int[] ids = recordService.SaveAnalyseResult(recordId, AnalyseResults
                                        .Select(x => new AnalyseResult()
                                        {
                                            Id = x.Id,
                                            ParameterRecordTypeId = x.ParameterRecordTypeId,
                                            Value = x.ResultText,
                                            UnitId = !SpecialValues.IsNewOrNonExisting(x.SelectedUnitId) ? x.SelectedUnitId : (int?)null,
                                            IsNormal = x.IsNormal,
                                            IsBelowRef = x.IsBelow,
                                            IsAboveRef = x.IsAbove,
                                            Details = x.Details
                                        }).ToArray());
            if (ids.Any())
            {
                ChangeTracker.AcceptChanges();
                ChangeTracker.IsEnabled = true;
                for (int i = 0; i < AnalyseResults.Count; i++)
                    AnalyseResults[i].Id = ids[i];
                LoadAnalyseResultView(recordId);
                return 1;
            }
            return SpecialValues.NonExistingId;
        }

        public void LoadProtocol(int assignmentId, int recordId, int visitId)
        {
            if (visitId > 0) return;
            ChangeTracker.IsEnabled = false;
            if (assignmentId > 0)
                LoadAssignmentData(assignmentId);
            else if (recordId > 0)
                LoadRecordData(recordId);
            ChangeTracker.IsEnabled = true;
        }

        #endregion

        #region Methods

        private bool ProtocolIsValid()
        {
            if (!AnalyseResults.Any() || AnalyseResults.All(x => x.ResultText.Trim() == string.Empty))
            {
                messageService.ShowWarning("Не заполнены результаты лабораторного исследования");
                return false;
            }
            return true;
        }    
             
        public string CanComplete()
        {
            string resStr = string.Empty;
            if (!AnalyseResults.Any() || AnalyseResults.All(x => x.ResultText.Trim() == string.Empty))
                resStr = "результаты лабораторного исследования";
            return resStr;
        }   

        private void LoadAssignmentData(int assignmentId)
        {           
            var assignment = recordService.GetAssignment(assignmentId).First();
            personId = assignment.PersonId;
            isMale = assignment.Person.IsMale;
            double days = assignment.AssignDateTime.Subtract(assignment.Person.BirthDate).TotalDays;
            years = (int)(days / 365);
            date = assignment.AssignDateTime;
            LoadAnalyseParameters(assignment.RecordTypeId);
        }

        private void LoadRecordData(int recordId)
        {
            var record = recordService.GetRecord(recordId).First();
            personId = record.PersonId;
            isMale = record.Person.IsMale;
            double days = record.ActualDateTime.Subtract(record.Person.BirthDate).TotalDays;
            years = (int)(days / 365);
            date = record.ActualDateTime;

            if (record.AnalyseResults.Any())
            {
                LoadAnalyseParameters(record.RecordTypeId);
                FillAnalyseResults(record.AnalyseResults.Select(x => new AnalyseParameterDTO() 
                    { 
                        Id = x.Id, 
                        ParameterRecordTypeId = x.ParameterRecordTypeId, 
                        Name = x.RecordType.Name, 
                        UnitId = x.UnitId, 
                        Result = x.Value,
                        Details = x.Details
                    }).ToArray());
                LoadAnalyseResultView(record.Id);
            }
        }
                
        private void FillAnalyseResults(AnalyseParameterDTO[] analyseParameterDTO)
        {
            foreach (var item in analyseParameterDTO)
            {
                var result = AnalyseResults.FirstOrDefault(x => x.ParameterRecordTypeId == item.ParameterRecordTypeId);
                if (result != null)
                {
                    result.Id = item.Id;
                    result.ResultText = item.Result;
                    result.SelectedUnitId = item.UnitId.HasValue ? item.UnitId.Value : SpecialValues.NonExistingId;
                    result.Details = item.Details;
                }
            }
        }
        
        private bool LoadAnalyseParameters(int recordTypeId)
        {
            var parameters = recordService.GetRecordTypeById(recordTypeId).First().RecordTypes1;
            if (parameters.Any())
            {               
                var result = parameters.Select(x => new AnalyseResultViewModel(recordService, cacheService) 
                            { 
                                RecordTypeId = recordTypeId,
                                IsMale = isMale,
                                Age = years,
                                Date = date,
                                ParameterRecordTypeId = x.Id,
                                ParameterName = x.Name,
                                Priority = x.Priority,
                                ResultText = string.Empty,
                                SelectedUnitId = SpecialValues.NonExistingId,
                                Details = string.Empty,
                                RefMin = x.AnalyseRefferences
                                        .Where(a => a.RecordTypeId == recordTypeId && a.IsMale == isMale && a.AgeFrom <= years && a.AgeTo >= years && a.BeginDateTime <= date && a.EndDateTime > date)
                                        .Select(a => a.RefMin).FirstOrDefault(),
                                RefMax = x.AnalyseRefferences
                                        .Where(a => a.RecordTypeId == recordTypeId && a.IsMale == isMale && a.AgeFrom <= years && a.AgeTo >= years && a.BeginDateTime <= date && a.EndDateTime > date)
                                        .Select(a => a.RefMax).FirstOrDefault()
                            }).ToArray();

                AnalyseResults.AddRange(result.OrderBy(x => x.Priority));
                return true;
            }
            else
            {
                messageService.ShowWarning("У данного исследования отсутствуют измеряемые параметры");
                return false;
            }
        }

        private void LoadAnalyseResultView(int recordId)
        {
            var record = recordService.GetRecord(recordId).First();
            AnalyseResultsView.Clear();
            AnalyseResultsView.AddRange(record.AnalyseResults
                    .Select(x => new AnalyseResultViewModel(recordService, cacheService)
                    {
                        RecordTypeId = record.RecordTypeId,
                        IsMale = isMale,
                        Age = years,
                        Date = date,
                        ParameterRecordTypeId = x.ParameterRecordTypeId,
                        ParameterName = x.RecordType.Name,
                        Priority = x.RecordType.Priority,
                        ResultText = x.Value,
                        UnitView = x.UnitId.HasValue ? x.Unit.ShortName : string.Empty,
                        Details = x.Details,
                        RefMin = x.RecordType.AnalyseRefferences
                                        .Where(a => a.RecordTypeId == record.RecordTypeId && a.IsMale == isMale && a.AgeFrom <= years && a.AgeTo >= years && a.BeginDateTime <= date && a.EndDateTime > date)
                                        .Select(a => a.RefMin).FirstOrDefault(),
                        RefMax = x.RecordType.AnalyseRefferences
                                .Where(a => a.RecordTypeId == record.RecordTypeId && a.IsMale == isMale && a.AgeFrom <= years && a.AgeTo >= years && a.BeginDateTime <= date && a.EndDateTime > date)
                                .Select(a => a.RefMax).FirstOrDefault()
                    }).ToArray().OrderBy(x => x.Priority));
            if (AnalyseResultsView.Any())
                SelectedAnalyseResultView = AnalyseResultsView.First();
            else
                ShowChart = false;
        }

        private void DrawChart(AnalyseResultViewModel analyseResult)
        {
            ChartData.Clear();
            RefMinData.Clear();
            RefMaxData.Clear();
            if (analyseResult.RefMax == 0.0 && analyseResult.RefMin == 0.0)
                ShowChart = false;
            else
            {
                var analysesQuery = recordService.GetAnalyseResults(personId, analyseResult.RecordTypeId, analyseResult.ParameterRecordTypeId);
                var analysesSelect = analysesQuery.Select(x => new { Date = x.Record.ActualDateTime, Value = x.Value, Unit = (x.UnitId.HasValue ? " " + x.Unit.ShortName : string.Empty) }).OrderBy(x => x.Date).ToArray();
                var dates = new List<DateTime>();
                foreach (var item in analysesSelect.OrderBy(x => x.Date))
                {
                    double result;
                    if (double.TryParse(item.Value, out result))
                    {
                        ChartData.Add(new ChartPoint() { ValueX = item.Date.ToShortDateString(), ValueY = result, Description = item.Date.ToString("dd.MM.yyyy HH:mm") + "\r\n" + result + item.Unit });
                        dates.Add(item.Date);
                    }
                }

                RefMinData.Add(new ChartPoint() { ValueX = dates.Min(x => x.Date).ToShortDateString(), ValueY = analyseResult.RefMin });
                RefMinData.Add(new ChartPoint() { ValueX = dates.Max(x => x.Date).ToShortDateString(), ValueY = analyseResult.RefMin });

                RefMaxData.Add(new ChartPoint() { ValueX = dates.Min(x => x.Date).ToShortDateString(), ValueY = analyseResult.RefMax });
                RefMaxData.Add(new ChartPoint() { ValueX = dates.Max(x => x.Date).ToShortDateString(), ValueY = analyseResult.RefMax });

                SelectedParameter = analyseResult.ParameterName;
                ShowChart = true;
            }
        }

        private async void SetAnalyseRefferences()
        {
            var refferencesViewModel = refferencesViewModelFactory();
            refferencesViewModel.Initialize(SelectedAnalyseResult.RecordTypeId, SelectedAnalyseResult.ParameterRecordTypeId);
            var result = await dialogService.ShowDialogAsync(refferencesViewModel);
            if (refferencesViewModel.SaveIsSuccessful)
            {

            }
        }

        public void Dispose()
        {
            ChangeTracker.Dispose();
            AnalyseResults.BeforeCollectionChanged -= OnBeforeDiagnosCollectionChanged;
        }
        #endregion

    }

    

}
