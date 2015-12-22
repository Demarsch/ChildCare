using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using Core.Extensions;
using Core.Misc;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using PatientInfoModule.Misc;
using PatientInfoModule.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Shell.Shared;

namespace PatientInfoModule.ViewModels
{
    public class InfoContentViewModel : TrackableBindableBase, INavigationAware, IActiveDataErrorInfo, IChangeTrackerMediator, IDisposable
    {
        private readonly IPatientService patientService;

        private readonly IEventAggregator eventAggregator;

        private readonly ILog log;

        private readonly PatientInfoViewModel patientInfo;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        private readonly Func<PatientInfoViewModel> relativeInfoFactory;

        public InfoContentViewModel(IPatientService patientService,
                                    IEventAggregator eventAggregator,
                                    ILog log,
                                    PatientInfoViewModel patientInfo,
                                    IRegionManager regionManager,
                                    IViewNameResolver viewNameResolver,
                                    Func<PatientInfoViewModel> relativeInfoFactory)
        {
            if (patientService == null)
            {
                throw new ArgumentNullException("patientService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (patientInfo == null)
            {
                throw new ArgumentNullException("patientInfo");
            }
            if (relativeInfoFactory == null)
            {
                throw new ArgumentNullException("relativeInfoFactory");
            }
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            if (viewNameResolver == null)
            {
                throw new ArgumentNullException("viewNameResolver");
            }
            this.patientService = patientService;
            this.eventAggregator = eventAggregator;
            this.log = log;
            this.patientInfo = patientInfo;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            this.relativeInfoFactory = relativeInfoFactory;
            currentPatientId = SpecialValues.NonExistingId;
            Relatives = new ObservableCollectionEx<PatientInfoViewModel>();
            Relatives.BeforeCollectionChanged += RelativesOnBeforeCollectionChanged;
            changeTracker = new CompositeChangeTracker(patientInfo.ChangeTracker, new ObservableCollectionChangeTracker<PatientInfoViewModel>(Relatives));
            changeTracker.PropertyChanged += OnChangesTracked;
            selectedPatientOrRelative = patientInfo;
            FailureMediator = new FailureMediator();
            createNewPatientCommand = new DelegateCommand(CreateNewPatient);
            saveChangesCommand = new DelegateCommand(async () => await SaveChangesAsync(), CanSaveChanges);
            cancelChangesCommand = new DelegateCommand(CancelChanges, CanCancelChanges);
            addRelativeCommand = new DelegateCommand(AddRelative, CanAddRelative);
            goBackToPatientCommand = new DelegateCommand(GoBackToPatient, CanGoBackToPatient);
            saveChangesCommandWrapper = new CommandWrapper { Command = SaveChangesCommand };
            loadRelativeListWrapper = new CommandWrapper { Command = new DelegateCommand(async () => await LoadPatientAndRelativesAsync(patientIdBeingLoaded)) };
            currentOperation = new TaskCompletionSource<object>();
            currentOperation.SetResult(null);
            eventAggregator.GetEvent<BeforeSelectionChangedEvent<Person>>().Subscribe(OnBeforePatientSelected);
        }

        private void OnBeforePatientSelected(BeforeSelectionChangedEventData data)
        {
            if (currentPatientId == SpecialValues.NewId || ChangeTracker.HasChanges)
            {
                data.AddActionToPerform(async () => await SaveChangesAsync(), () => regionManager.RequestNavigate(RegionNames.ModuleList, viewNameResolver.Resolve<InfoHeaderViewModel>()));
            }
        }

        private TaskCompletionSource<object> currentOperation;

        public FailureMediator FailureMediator { get; private set; }

        private void OnChangesTracked(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName) || string.CompareOrdinal(e.PropertyName, "HasChanges") == 0)
            {
                UpdateCommandsState();
            }
        }

        private void UpdateCommandsState()
        {
            saveChangesCommand.RaiseCanExecuteChanged();
            cancelChangesCommand.RaiseCanExecuteChanged();
            addRelativeCommand.RaiseCanExecuteChanged();
        }

        #region Actions

        private readonly DelegateCommand createNewPatientCommand;

        private readonly DelegateCommand saveChangesCommand;

        private readonly DelegateCommand cancelChangesCommand;

        private readonly DelegateCommand addRelativeCommand;

        private readonly CommandWrapper saveChangesCommandWrapper;

        private readonly CommandWrapper loadRelativeListWrapper;

        public ICommand CreateNewPatientCommand
        {
            get { return createNewPatientCommand; }
        }

        private void CreateNewPatient()
        {
            eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Publish(SpecialValues.NewId);
        }

        public ICommand SaveChangesCommand
        {
            get { return saveChangesCommand; }
        }

        private async Task<bool> SaveChangesAsync()
        {
            await currentOperation.Task;
            FailureMediator.Deactivate();
            if (!Validate())
            {
                return false;
            }
            currentOperation = new TaskCompletionSource<object>();
            PatientInfoViewModel personToSave = null;
            try
            {
                log.InfoFormat("Saving relatives data for patient with Id = {0}", currentPatientId == SpecialValues.NewId ? "(New patient)" : currentPatientId.ToString());
                var isNewPatient = patientInfo.CurrentPerson.Id.IsNewOrNonExisting();
                personToSave = patientInfo;
                await patientInfo.SaveChangesAsync();
                if (isNewPatient)
                {
                    currentPatientId = patientInfo.CurrentPerson.Id;
                    eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Publish(currentPatientId);
                }
                foreach (var relative in Relatives.Where(x => x.CurrentPerson.Id.IsNewOrNonExisting() || x.ChangeTracker.HasChanges))
                {
                    personToSave = relative;
                    await relative.SaveChangesAsync(patientInfo.CurrentPerson);
                }
                personToSave = null;
                CancelValidation();
                ChangeTracker.AcceptChanges();
                ChangeTracker.IsEnabled = true;
                UpdateCommandsState();
                return true;
            }
            catch (OperationCanceledException)
            {
                return true;
            }
            catch (Exception ex)
            {
                //At this point patint himself is already saved
                log.ErrorFormatEx(ex, "Failed to save relative relationship for patient with Id {0}", patientInfo.CurrentPerson.Id);
                FailureMediator.Activate("Не удалось сохранить родственников пациента. Попробуйте еще раз. Если ошибка повторится, пожалуйста, обратитесь в службу поддержки",
                                         saveChangesCommandWrapper,
                                         ex,
                                         true);
                SelectedPatientOrRelative = personToSave;
                return false;
            }
            finally
            {
                currentOperation.SetResult(null);
            }
        }

        private bool CanSaveChanges()
        {
            if (currentPatientId == SpecialValues.NewId)
            {
                return true;
            }
            return ChangeTracker.HasChanges;
        }

        public ICommand CancelChangesCommand
        {
            get { return cancelChangesCommand; }
        }

        private void CancelChanges()
        {
            FailureMediator.Deactivate();
            ChangeTracker.RestoreChanges();
            CancelValidation();
            if (SelectedPatientOrRelative.IsRelative && SelectedPatientOrRelative.CurrentPerson.Id.IsNewOrNonExisting())
            {
                SelectedPatientOrRelative = patientInfo;
            }
        }

        private bool CanCancelChanges()
        {
            if (currentPatientId == SpecialValues.NewId)
            {
                return false;
            }
            return ChangeTracker.HasChanges;
        }

        public ICommand AddRelativeCommand
        {
            get { return addRelativeCommand; }
        }

        private async void AddRelative()
        {
            var newRelative = relativeInfoFactory();
            newRelative.IsRelative = true;
            newRelative.PersonRelative = new PersonRelative
                                         {
                                             IsRepresentative = !Relatives.Any(x => x.IsRepresentative),
                                             PersonId = patientInfo.CurrentPerson.Id,
                                         };
            Relatives.Add(newRelative);
            SelectedPatientOrRelative = newRelative;
            await newRelative.LoadPatientInfoAsync(SpecialValues.NewId);
        }

        private bool CanAddRelative()
        {
            return currentPatientId != SpecialValues.NonExistingId;
        }

        private readonly DelegateCommand goBackToPatientCommand;

        public ICommand GoBackToPatientCommand { get { return goBackToPatientCommand; } }

        private void GoBackToPatient()
        {
            SelectedPatientOrRelative = patientInfo;
        }

        private bool CanGoBackToPatient()
        {
            return selectedPatientOrRelative != patientInfo;
        }

        #endregion

        private void RelativesOnBeforeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.Cast<PatientInfoViewModel>())
                {
                    oldItem.PropertyChanged -= OnRelativeIsRepresentiveChanged;
                    changeTracker.RemoveTracker(oldItem.ChangeTracker);
                    oldItem.CurrentPatientHasRelativeCheckRequired -= OnCurrentPatientHasRelativeCheckRequired;
                    oldItem.RelativeNavigationRequested -= OnRelativeNavigationRequested;
                    oldItem.RelativeRemoveRequested -= OnRelativeRemoveRequested;
                    oldItem.Dispose();
                }
            }
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<PatientInfoViewModel>())
                {
                    newItem.PropertyChanged += OnRelativeIsRepresentiveChanged;
                    changeTracker.AddTracker(newItem.ChangeTracker);
                    newItem.CurrentPatientHasRelativeCheckRequired += OnCurrentPatientHasRelativeCheckRequired;
                    newItem.RelativeNavigationRequested += OnRelativeNavigationRequested;
                    newItem.RelativeRemoveRequested += OnRelativeRemoveRequested;
                }
            }
        }

        private void OnRelativeRemoveRequested(object sender, EventArgs eventArgs)
        {
            Relatives.Remove((PatientInfoViewModel)sender);
        }

        private void OnRelativeNavigationRequested(object sender, DataEventArgs<int> e)
        {
            var relative = Relatives.FirstOrDefault(x => x.CurrentPerson != null
                                                         && !x.CurrentPerson.Id.IsNewOrNonExisting()
                                                         && x.CurrentPerson.Id == e.Value);
            if (relative != null)
            {
                SelectedPatientOrRelative = relative;
            }
        }

        private void OnCurrentPatientHasRelativeCheckRequired(object sender, CurrentPatientHasRelativeCheckEventArgs e)
        {
            e.HasThisRelative = Relatives.Any(x => x.CurrentPerson != null 
                                              && !x.CurrentPerson.Id.IsNewOrNonExisting() 
                                              && x.CurrentPerson.Id == e.RelativeId);
        }

        private void OnRelativeIsRepresentiveChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || string.CompareOrdinal(e.PropertyName, "IsRepresentative") == 0)
            {
                var thisRelative = (PatientInfoViewModel)sender;
                if (!thisRelative.IsRepresentative)
                {
                    return;
                }
                foreach (var otherRelative in Relatives.Where(x => !ReferenceEquals(x, thisRelative)))
                {
                    otherRelative.IsRepresentative = false;
                }
            }
        }

        public ObservableCollectionEx<PatientInfoViewModel> Relatives { get; private set; }

        private PatientInfoViewModel selectedPatientOrRelative;

        public PatientInfoViewModel SelectedPatientOrRelative
        {
            get { return selectedPatientOrRelative; }
            set
            {
                value = value ?? patientInfo;
                SetProperty(ref selectedPatientOrRelative, value);
                goBackToPatientCommand.RaiseCanExecuteChanged();
            }
        }

        private int currentPatientId;

        private int patientIdBeingLoaded;

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            var targetPatientId = (int?)navigationContext.Parameters[ParameterNames.PatientId] ?? SpecialValues.NonExistingId;
            await LoadPatientAndRelativesAsync(targetPatientId);
        }

        private async Task LoadPatientAndRelativesAsync(int targetPatientId)
        {
            await currentOperation.Task;
            if (targetPatientId == currentPatientId)
            {
                return;
            }
            currentOperation = new TaskCompletionSource<object>();
            try
            {
                log.InfoFormat("Loading patient and relative info for patient with Id {0}...", targetPatientId);
                ChangeTracker.IsEnabled = false;
                patientIdBeingLoaded = targetPatientId;
                await Task.WhenAll(patientInfo.LoadPatientInfoAsync(targetPatientId), LoadRelativesAsync(targetPatientId));
                currentPatientId = targetPatientId;
                patientIdBeingLoaded = SpecialValues.NonExistingId;
                ChangeTracker.IsEnabled = true;
                UpdateCommandsState();
                SelectedPatientOrRelative = patientInfo;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to load common and relative info for patient with Id {0}", targetPatientId);
                FailureMediator.Activate("Не удалось загрузить данные пациента. Попробуйте еще раз или обратитесь в службу поддержки", loadRelativeListWrapper, ex);
                SelectedPatientOrRelative = patientInfo.FailureMediator.IsActive ? patientInfo : Relatives.FirstOrDefault(x => x.FailureMediator.IsActive);
            }
            finally
            {
                currentOperation.SetResult(null);
            }
        }

        private async Task LoadRelativesAsync(int patientId)
        {
            Relatives.Clear();
            if (patientId.IsNewOrNonExisting())
            {
                return;
            }
            log.InfoFormat("Loading relative list for patient with Id {0}...", patientId);
            var relatives = await patientService.GetRelativesAsync(patientId);
            var tasks = new List<Task>();
            foreach (var relative in relatives)
            {
                var viewModel = relativeInfoFactory();
                viewModel.PersonRelative = relative;
                tasks.Add(viewModel.LoadPatientInfoAsync(relative.RelativeId));
                Relatives.Add(viewModel);
            }
            await Task.WhenAll(tasks);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public bool Validate()
        {
            PatientInfoViewModel invalidPerson = null;
            var result = patientInfo.Validate();
            if (!result)
            {
                invalidPerson = patientInfo;
            }
            foreach (var relative in Relatives)
            {
                result &= relative.Validate();
                if (!result && invalidPerson == null)
                {
                    invalidPerson = relative;
                }
            }
            if (invalidPerson != null)
            {
                SelectedPatientOrRelative = invalidPerson;
            }
            return result;
        }

        public void CancelValidation()
        {
            patientInfo.CancelValidation();
            foreach (var relative in Relatives)
            {
                relative.CancelValidation();
            }
        }

        private readonly CompositeChangeTracker changeTracker;

        public IChangeTracker ChangeTracker { get { return changeTracker; } }

        public void Dispose()
        {
            ChangeTracker.Dispose();
            Relatives.BeforeCollectionChanged -= RelativesOnBeforeCollectionChanged;
            eventAggregator.GetEvent<BeforeSelectionChangedEvent<Person>>().Unsubscribe(OnBeforePatientSelected);
        }
    }
}