using Core.Data;
using Core.Data.Misc;
using Core.Misc;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using PatientRecordsModule.Services;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PatientRecordsModule.ViewModels
{
    public class DiagnosesCollectionViewModel : BindableBase, IDisposable, IChangeTrackerMediator
    {
        private readonly IDiagnosService diagnosService;
        private readonly IRecordService recordService;
        private readonly ILog logService;
        private readonly ICacheService cacheService;
        private readonly IDialogServiceAsync dialogService;
        private readonly IDialogService messageService;
        private CancellationTokenSource currentLoadingToken;
        private readonly CompositeChangeTracker changeTracker;

        public DiagnosesCollectionViewModel(IDiagnosService diagnosService, IRecordService recordService, ICacheService cacheService, IDialogServiceAsync dialogService, 
                                            IDialogService messageService, ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (diagnosService == null)
            {
                throw new ArgumentNullException("diagnosService");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }
            this.diagnosService = diagnosService;
            this.recordService = recordService;
            this.logService = logService;
            this.cacheService = cacheService;
            this.dialogService = dialogService;
            this.messageService = messageService;

            addDiagnosCommand = new DelegateCommand<int?>(AddDiagnos);
            removeDiagnosCommand = new DelegateCommand(RemoveDiagnos);
            makeClarificationCommand = new DelegateCommand(MakeClarification);

            DiagnosLevels = new ObservableCollectionEx<DiagnosLevelViewModel>();

            Diagnoses = new ObservableCollectionEx<DiagnosViewModel>();
            Diagnoses.BeforeCollectionChanged += OnBeforeDiagnosCollectionChanged;

            changeTracker = new CompositeChangeTracker(new ObservableCollectionChangeTracker<DiagnosViewModel>(Diagnoses));
        }

        #region Methods
        /// <summary>
        /// Load record diagnoses
        /// </summary>
        /// <param name="recordId">0 - if there is no </param>
        internal async void Load(string diagnosTypeOption, int recordId = 0)
        {
            bool loadingIsCompleted = false;
            IDisposableQueryable<DiagnosLevel> diagnosLevelsQuery = null;
            IDisposableQueryable<Diagnosis> diagnosesQuery = null;
            try
            {                
                DiagnosLevels.Clear();                
                diagnosLevelsQuery = diagnosService.GetActualDiagnosLevels();
                DiagnosLevels.AddRange(diagnosLevelsQuery.Select(x => new DiagnosLevelViewModel() { Id = x.Id, Name = x.Name }).ToArray());

                var diagnosType = diagnosService.GetDiagnosTypeByOption(diagnosTypeOption).First();
                NeedSetMainDiagnos = diagnosType.NeedSetMainDiagnos;
                MainDiagnosHeader = diagnosType.MainDiagnosHeader;

                if (recordId != 0)
                {
                    Diagnoses.Clear();
                    if (currentLoadingToken != null)
                    {
                        currentLoadingToken.Cancel();
                        currentLoadingToken.Dispose();
                    }
                    currentLoadingToken = new CancellationTokenSource();
                    var token = currentLoadingToken.Token;                    

                    ChangeTracker.IsEnabled = false;
                    diagnosesQuery = diagnosService.GetRecordDiagnos(recordId);
                    var result = await Task.Factory.StartNew(() =>
                    {
                        return diagnosesQuery.Select(x => new DiagnosViewModel()
                        {
                            Id = x.Id,
                            DiagnosText = x.DiagnosText,
                            MKB = x.MKB,
                            LevelId = x.DiagnosLevelId,
                            LevelName = x.DiagnosLevel.Name,
                            LevelPriority = x.DiagnosLevel.Priority,
                            IsComplication = x.DiagnosLevel.IsComplication,
                            HasMKB = x.DiagnosLevel.HasMKB,
                            AllowClarification = x.DiagnosLevel.HasMKB || x.DiagnosLevel.IsComplication,
                            ComplicationId = x.ComplicationId,
                            IsMainDiagnos = x.IsMainDiagnos
                        }).ToArray();
                    }, token);

                    Diagnoses.AddRange(result.OrderBy(x => x.LevelPriority));
                    SetVisibilityGridData();
                }
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                logService.Error("Failed to load diagnoses", ex);
                loadingIsCompleted = false;
            }
            finally
            {
                if (diagnosLevelsQuery != null)
                    diagnosLevelsQuery.Dispose();
                if (diagnosesQuery != null)
                    diagnosesQuery.Dispose();
                ChangeTracker.IsEnabled = true;
            }            
        }        

        private void SetVisibilityGridData()
        {
            HasAnyDiagnoses = Diagnoses.Any();            
        }

        private void AddDiagnos(int? levelId)
        {
            AddDiagnosLevel(levelId);
        }

        private void AddDiagnosLevel(int? levelId, int? complicationId = null, string diagnosText = "")
        {
            if (!levelId.HasValue) return;
            var level = diagnosService.GetDiagnosLevelById(levelId.Value).First();

            int insertPos = -1;
            for (int i = 0; i < Diagnoses.Count; i++)
            {
                if (Diagnoses[i].LevelPriority > level.Priority)
                {
                    insertPos = i;
                    break;
                }
            }

            var insertDiagnos = new DiagnosViewModel()
            {
                Id = SpecialValues.NewId,
                DiagnosText = diagnosText,
                MKB = string.Empty,
                LevelId = level.Id,
                LevelPriority = level.Priority,
                LevelName = level.Name,
                ComplicationId = complicationId,
                IsComplication = level.IsComplication,
                IsMainDiagnos = false,
                HasMKB = level.HasMKB,
                AllowClarification = level.HasMKB || level.IsComplication

            };

            Diagnoses.Insert((Diagnoses.Any() && insertPos != -1) ? insertPos : 0, insertDiagnos);
            SetVisibilityGridData();
        }

        private void RemoveDiagnos()
        {
            if (SelectedDiagnos == null) return;
            if (messageService.AskUser("Удалить диагноз '" + SelectedDiagnos.LevelName + "' ?") == true)
            {
                string exception = string.Empty;
                if (SelectedDiagnos.Id != SpecialValues.NewId)
                    diagnosService.DeleteDiagnos(SelectedDiagnos.Id, out exception);
                Diagnoses.Remove(SelectedDiagnos);
                SetVisibilityGridData();
            };            
        }

        private async void MakeClarification()
        {
            if (!SelectedDiagnos.IsComplication)
            {
                using (var viewModel = new MKBTreeViewModel(diagnosService, cacheService))
                {
                    viewModel.Initialize();
                    var result = await dialogService.ShowDialogAsync(viewModel);
                    if (result == true)
                    {
                        var selectedMKB = viewModel.MKBTree.Any(x => x.IsSelected) ? viewModel.MKBTree.First(x => x.IsSelected) : 
                                                                                     viewModel.MKBTree.SelectMany(x => x.Children).First(x => x.IsSelected);
                        SelectedDiagnos.MKB = selectedMKB.Code;
                    }
                }
            }
            else
            {
                using (var viewModel = new ComplicationsTreeViewModel(diagnosService, cacheService))
                {
                    viewModel.Initialize();
                    var result = await dialogService.ShowDialogAsync(viewModel);
                    if (result == true)
                    {
                        var checkedComplications = viewModel.CheckedComplications;
                        SelectedDiagnos.ComplicationId = checkedComplications.First().Id;
                        SelectedDiagnos.DiagnosText = checkedComplications.First().Name;
                        for (int i = 1; i < checkedComplications.Count; i++)
                            AddDiagnosLevel(SelectedDiagnos.LevelId, checkedComplications[i].Id, checkedComplications[i].Name);                           
                    }
                }
            }
        }

        public void Dispose()
        {
            ChangeTracker.Dispose();
            foreach (var diagnos in Diagnoses)
            {
                diagnos.PropertyChanged -= OnDiagnosPropertyChanged;
            }
            Diagnoses.BeforeCollectionChanged -= OnBeforeDiagnosCollectionChanged;
        }

        #endregion

        #region Properties

        public ObservableCollectionEx<DiagnosViewModel> Diagnoses { get; private set; }

        private ObservableCollectionEx<DiagnosLevelViewModel> diagnosLevels;
        public ObservableCollectionEx<DiagnosLevelViewModel> DiagnosLevels
        {
            get { return diagnosLevels; }
            set { SetProperty(ref diagnosLevels, value); }
        }

        private DiagnosViewModel selectedDiagnos;
        public DiagnosViewModel SelectedDiagnos
        {
            get { return selectedDiagnos; }
            set { SetProperty(ref selectedDiagnos, value); }
        }

        private bool needSetMainDiagnos;
        public bool NeedSetMainDiagnos
        {
            get { return needSetMainDiagnos; }
            set { SetProperty(ref needSetMainDiagnos, value); }
        }             

        private bool hasAnyDiagnoses;
        public bool HasAnyDiagnoses
        {
            get { return hasAnyDiagnoses; }
            set { SetProperty(ref hasAnyDiagnoses, value); }
        }

        private string mainDiagnosHeader;
        public string MainDiagnosHeader
        {
            get { return mainDiagnosHeader; }
            set { SetProperty(ref mainDiagnosHeader, value); }
        }

        private readonly DelegateCommand<int?> addDiagnosCommand;
        public ICommand AddDiagnosCommand { get { return addDiagnosCommand; } }

        private readonly DelegateCommand removeDiagnosCommand;
        public ICommand RemoveDiagnosCommand { get { return removeDiagnosCommand; } }

        private readonly DelegateCommand makeClarificationCommand;
        public ICommand MakeClarificationCommand { get { return makeClarificationCommand; } }

        public IChangeTracker ChangeTracker
        {
            get { return changeTracker; }
        }

        #endregion
        
        #region Events

        void OnBeforeDiagnosCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<DiagnosViewModel>())
                {
                    newItem.PropertyChanged += OnDiagnosPropertyChanged;
                    changeTracker.AddTracker(newItem.ChangeTracker);
                }
            }
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.Cast<DiagnosViewModel>())
                {
                    oldItem.PropertyChanged -= OnDiagnosPropertyChanged;
                    changeTracker.RemoveTracker(oldItem.ChangeTracker);
                }
            }
        }

        private void OnDiagnosPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (string.IsNullOrEmpty(propertyChangedEventArgs.PropertyName) || string.CompareOrdinal(propertyChangedEventArgs.PropertyName, "DiagnosText") == 0)
            {
            }
        }

        #endregion


    }
}
