using Core.Data;
using Core.Data.Misc;
using Core.Misc;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
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
        private CancellationTokenSource currentLoadingToken;
        public InteractionRequest<Confirmation> ConfirmationInteractionRequest { get; private set; }
        public InteractionRequest<Notification> NotificationInteractionRequest { get; private set; }
        private readonly CompositeChangeTracker changeTracker;

        public DiagnosesCollectionViewModel(IDiagnosService diagnosService, IRecordService recordService, ILog logService)
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
            this.diagnosService = diagnosService;
            this.recordService = recordService;
            this.logService = logService;

            addDiagnosCommand = new DelegateCommand<int?>(AddDiagnos);
            removeDiagnosCommand = new DelegateCommand(RemoveDiagnos);
            selectMKBCommand = new DelegateCommand(SelectMKB);

            ConfirmationInteractionRequest = new InteractionRequest<Confirmation>();
            NotificationInteractionRequest = new InteractionRequest<Notification>();

            Diagnoses = new ObservableCollectionEx<DiagnosViewModel>();
            Diagnoses.CollectionChanged += OnDiagnosCollectionChanged;
            Diagnoses.BeforeCollectionChanged += OnBeforeDiagnosCollectionChanged;

            changeTracker = new CompositeChangeTracker(new ObservableCollectionChangeTracker<DiagnosViewModel>(Diagnoses));
        }

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

        void OnDiagnosCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
          
        }
      
        /// <summary>
        /// Load record diagnoses
        /// </summary>
        /// <param name="recordId">0 - if there is no </param>
        internal async void Load(int recordId)
        {
            Diagnoses.Clear();
            if (SpecialValues.IsNewOrNonExisting(recordId))
            {
                SetVisibilityControlButtons(false);
                return;
            }
            if (currentLoadingToken != null)
            {
                currentLoadingToken.Cancel();
                currentLoadingToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            IDisposableQueryable<Diagnosis> diagnosesQuery = null;
            IDisposableQueryable<DiagnosLevel> diagnosLevelsQuery = null;
            ChangeTracker.IsEnabled = false;
            try
            {
                diagnosLevelsQuery = diagnosService.GetActualDiagnosLevels();
                DiagnosLevels = new ObservableCollectionEx<DiagnosLevelViewModel>(diagnosLevelsQuery.Select(x => new DiagnosLevelViewModel() { Id = x.Id, Name = x.Name }).ToArray());

                diagnosesQuery = diagnosService.GetRecordDiagnos(recordId);
                var result = await Task.Factory.StartNew(() =>
                {
                    return diagnosesQuery.Select(x => new DiagnosViewModel()
                    { 
                        Id = x.Id,
                        DiagnosText = x.DiagnosText,
                        MKB = x.MKB,
                        LevelId = x.DiagnosLevelId,
                        LevelPriority = x.DiagnosLevel.Priority,
                        LevelName = x.DiagnosLevel.Name,
                        ComplicationId = x.ComplicationId,
                        IsMainDiagnos = x.IsMainDiagnos,
                        NeedSetMainDiagnos = x.PersonDiagnos.DiagnosType.NeedSetMainDiagnos
                    }).ToArray();
                }, token);
               
                Diagnoses.AddRange(result.OrderBy(x => x.LevelPriority));
                SetVisibilityGridData();
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
                SetVisibilityControlButtons(loadingIsCompleted);
                if (diagnosLevelsQuery != null)
                    diagnosLevelsQuery.Dispose();
                if (diagnosesQuery != null)
                    diagnosesQuery.Dispose();
                ChangeTracker.IsEnabled = true;
            }            
        }

        private void SetVisibilityControlButtons(bool allowEdit)
        {
            AllowAddDiagnos = AllowRemoveDiagnos = allowEdit;
        }

        private void SetVisibilityGridData()
        {
            HasAnyDiagnoses = Diagnoses.Any();
            NeedSetMainDiagnos = Diagnoses.Any(x => x.NeedSetMainDiagnos);
        }

        private void AddDiagnos(int? levelId)
        {
            if (!levelId.HasValue) return;
            var level = diagnosService.GetDiagnosLevelById(levelId.Value).First();
            
            int insertPos = -1;
            for (int i = 0; i < diagnoses.Count; i++)
			{
			    if (diagnoses[i].LevelPriority > level.Priority)
                {
                    insertPos = i;
                    break;
                }
			}

            var insertDiagnos = new DiagnosViewModel()
                    {
                        Id = SpecialValues.NewId,
                        DiagnosText = string.Empty,
                        MKB = string.Empty,
                        LevelId = level.Id,
                        LevelPriority = level.Priority,
                        LevelName = level.Name,
                        ComplicationId = (int?)null,
                        IsMainDiagnos = false
                    };

            Diagnoses.Insert((diagnoses.Any() && insertPos != -1) ? insertPos : 0, insertDiagnos);
            SetVisibilityGridData();
        }

        private void RemoveDiagnos()
        {
            if (SelectedDiagnos == null) return;
            ConfirmationInteractionRequest.Raise(new Confirmation()
                {
                    Title = "Внимание",
                    Content = "Удалить диагноз '" + SelectedDiagnos.LevelName + "' ?"
                }, OnDeleteDiagnosDialogClosed);            
        }

        private void SelectMKB()
        {
            //SelectedDiagnos;
            return;
        }

        private void OnDeleteDiagnosDialogClosed(Confirmation dialog)
        {
            if (dialog.Confirmed)
            {
                string exception = string.Empty;
                if (diagnosService.DeleteDiagnos(SelectedDiagnos.Id, out exception))
                {
                    Diagnoses.Remove(SelectedDiagnos);
                    SetVisibilityGridData();
                }
            }
        }

        private ObservableCollectionEx<DiagnosViewModel> diagnoses;
        public ObservableCollectionEx<DiagnosViewModel> Diagnoses
        {
            get { return diagnoses; }
            set { SetProperty(ref diagnoses, value); }
        }

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

        private bool allowAddDiagnos;
        public bool AllowAddDiagnos
        {
            get { return allowAddDiagnos; }
            set { SetProperty(ref allowAddDiagnos, value); }
        }

        private bool allowRemoveDiagnos;
        public bool AllowRemoveDiagnos
        {
            get { return allowRemoveDiagnos; }
            set { SetProperty(ref allowRemoveDiagnos, value); }
        }

        private bool hasAnyDiagnoses;
        public bool HasAnyDiagnoses
        {
            get { return hasAnyDiagnoses; }
            set { SetProperty(ref hasAnyDiagnoses, value); }
        }

        private readonly DelegateCommand<int?> addDiagnosCommand;
        public ICommand AddDiagnosCommand { get { return addDiagnosCommand; } }

        private readonly DelegateCommand removeDiagnosCommand;
        public ICommand RemoveDiagnosCommand { get { return removeDiagnosCommand; } }

        private readonly DelegateCommand selectMKBCommand;
        public ICommand SelectMKBCommand { get { return selectMKBCommand; } }

        public void Dispose()
        {
            ChangeTracker.Dispose();
            foreach (var diagnos in Diagnoses)
            {
                diagnos.PropertyChanged -= OnDiagnosPropertyChanged;
            }
            Diagnoses.BeforeCollectionChanged -= OnBeforeDiagnosCollectionChanged;
            Diagnoses.CollectionChanged -= OnDiagnosCollectionChanged;
        }

        private void OnDiagnosDeleteRequested(object sender, EventArgs e)
        {
            Diagnoses.Remove(sender as DiagnosViewModel);
        }

        private void OnDiagnosPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (string.IsNullOrEmpty(propertyChangedEventArgs.PropertyName) || string.CompareOrdinal(propertyChangedEventArgs.PropertyName, "StringRepresentation") == 0)
            {
            }
        }


        public IChangeTracker ChangeTracker
        {
            get { return changeTracker; }
        }
    }
}
