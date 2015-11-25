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

            addDiagnosLevelCommand = new DelegateCommand<int?>(AddDiagnosLevel);
            removeDiagnosLevelCommand = new DelegateCommand<int?>(RemoveDiagnosLevel);

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
                        DiagnosId = x.Id, 
                        RecordId = x.PersonDiagnos.RecordId,
                        RecordName = x.PersonDiagnos.Record.RecordType.Name,
                        DiagnosTypeId = x.PersonDiagnos.DiagnosTypeId,
                        DiagnosTypeName = x.PersonDiagnos.DiagnosType.Name,
                        DiagnosText = x.DiagnosText,
                        MKB = x.MKB,
                        DiagnosLevelId = x.DiagnosLevelId,
                        DiagnosLevelName = x.DiagnosLevel.Name,
                        ComplicationId = x.ComplicationId,
                        IsMainDiagnos = x.IsMainDiagnos,
                        NeedSelectMainDiagnos = x.PersonDiagnos.DiagnosType.NeedSetMainDiagnos
                    }).ToArray();
                }, token);
               
                Diagnoses.AddRange(result);
                HasAnyDiagnoses = Diagnoses.Any();

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
                if (loadingIsCompleted)
                   SetVisibilityControlButtons(true);
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

        private void AddDiagnosLevel(int? levelId)
        {
            return;
        }

        private void RemoveDiagnosLevel(int? diagnosId)
        {
            return;
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

        private readonly DelegateCommand<int?> addDiagnosLevelCommand;
        public ICommand AddDiagnosLevelCommand { get { return addDiagnosLevelCommand; } }

        private readonly DelegateCommand<int?> removeDiagnosLevelCommand;
        public ICommand RemoveDiagnosLevelCommand { get { return removeDiagnosLevelCommand; } }

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
