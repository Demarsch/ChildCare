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
using Shared.Patient.ViewModels;
using Shell.Shared;
using Core.Services;
using Core.Reports;

namespace PatientInfoModule.ViewModels
{
    public class InfoContentViewModel : TrackableBindableBase, INavigationAware, IActiveDataErrorInfo, IChangeTrackerMediator, IDisposable
    {
        private readonly IPatientService patientService;

        private readonly ISecurityService securityService;

        private readonly IEventAggregator eventAggregator;

        private readonly ILog log;

        private readonly IDialogServiceAsync dialogService;

        private readonly PatientInfoViewModel patientInfo;

        private readonly IRegionManager regionManager;

        private readonly IRecordService recordService;

        private readonly IViewNameResolver viewNameResolver;

        private readonly IReportGeneratorHelper reportGenerator;

        private readonly Func<PatientInfoViewModel> relativeInfoFactory;

        private readonly Func<AgreementsCollectionViewModel> agreementsCollectionFactory;

        private readonly Func<PersonSearchDialogViewModel> relativeSearchFactory;

        public InfoContentViewModel(IPatientService patientService,
                                    ISecurityService securityService,
                                    IEventAggregator eventAggregator,
                                    ILog log,
                                    IDialogServiceAsync dialogService,
                                    IRecordService recordService,
                                    PatientInfoViewModel patientInfo,
                                    PatientAssignmentListViewModel patientAssignmentList,
                                    IRegionManager regionManager,
                                    IViewNameResolver viewNameResolver,
                                    Func<PatientInfoViewModel> relativeInfoFactory,
                                    Func<AgreementsCollectionViewModel> agreementsCollectionFactory,
                                    Func<PersonSearchDialogViewModel> relativeSearchFactory,
                                    IReportGeneratorHelper reportGenerator)
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
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            if (patientInfo == null)
            {
                throw new ArgumentNullException("patientInfo");
            }
            if (agreementsCollectionFactory == null)
            {
                throw new ArgumentNullException("agreementsCollectionFactory");
            }
            if (patientAssignmentList == null)
            {
                throw new ArgumentNullException("patientAssignmentList");
            }           
            if (relativeInfoFactory == null)
            {
                throw new ArgumentNullException("relativeInfoFactory");
            }
            if (relativeSearchFactory == null)
            {
                throw new ArgumentNullException("relativeSearchFactory");
            }
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            if (viewNameResolver == null)
            {
                throw new ArgumentNullException("viewNameResolver");
            }
            if (securityService == null)
            {
                throw new ArgumentNullException("securityService");
            }
            if (reportGenerator == null)
            {
                throw new ArgumentNullException("reportGenerator");
            }
            this.securityService = securityService;
            this.patientService = patientService;
            this.eventAggregator = eventAggregator;
            this.log = log;
            this.dialogService = dialogService;
            this.recordService = recordService;
            this.patientInfo = patientInfo;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            this.agreementsCollectionFactory = agreementsCollectionFactory;
            this.relativeInfoFactory = relativeInfoFactory;
            this.relativeSearchFactory = relativeSearchFactory;
            PatientAssignmentListViewModel = patientAssignmentList;
            this.reportGenerator = reportGenerator;
            currentPatientId = PatientAssignmentListViewModel.PatientId = SpecialValues.NonExistingId;            
            Relatives = new ObservableCollectionEx<PatientInfoViewModel>();
            Relatives.BeforeCollectionChanged += RelativesOnBeforeCollectionChanged;
            changeTracker = new CompositeChangeTracker(patientInfo.ChangeTracker, new ObservableCollectionChangeTracker<PatientInfoViewModel>(Relatives));
            changeTracker.PropertyChanged += OnChangesTracked;
            selectedPatientOrRelative = patientInfo;
            FailureMediator = new FailureMediator();
            createNewPatientCommand = new DelegateCommand(CreateNewPatient);
            saveChangesCommand = new DelegateCommand(async () => await SaveChangesAsync(), CanSaveChanges);
            cancelChangesCommand = new DelegateCommand(CancelChanges, CanCancelChanges);
            addRelativeCommand = new DelegateCommand(AddRelativeAsync, CanAddRelative);
            searchRelativeCommand = new DelegateCommand(SearchRelativeAsync, CanAddRelative);
            goBackToPatientCommand = new DelegateCommand(GoBackToPatient, CanGoBackToPatient);
            createAmbCardCommand = new DelegateCommand(CreateAmbCardAsync, CanCreateAmbCard);
            deleteAmbCardCommand = new DelegateCommand(DeleteAmbCardAsync, CanDeleteAmbCard);
            printAmbCardCommand = new DelegateCommand(PrintAmbCardAsync, CanPrintAmbCard);
            showAgreementsCommand = new DelegateCommand(ShowAgreementsAsync, CanShowAgreements);
            saveChangesCommandWrapper = new CommandWrapper { Command = SaveChangesCommand };
            recreateAmbCardWrapper = new CommandWrapper { Command = CreateAmbCardCommand };
            redeleteAmbCardWrapper = new CommandWrapper { Command = DeleteAmbCardCommand };
            loadRelativeListWrapper = new CommandWrapper { Command = new DelegateCommand(async () => await LoadPatientAndRelativesAsync(patientIdBeingLoaded)) };
            currentOperation = new TaskCompletionSource<object>();
            currentOperation.SetResult(null);
            eventAggregator.GetEvent<BeforeSelectionChangedEvent<Person>>().Subscribe(OnBeforePatientSelected);
        }

        public PatientAssignmentListViewModel PatientAssignmentListViewModel { get; private set; }

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
            searchRelativeCommand.RaiseCanExecuteChanged();
            showAgreementsCommand.RaiseCanExecuteChanged();
        }

        private void UpdateAmbCardCommandsState()
        {
            createAmbCardCommand.RaiseCanExecuteChanged();
            deleteAmbCardCommand.RaiseCanExecuteChanged();
            printAmbCardCommand.RaiseCanExecuteChanged();
            OnPropertyChanged(() => CanViewCreateAmbCardButton);
            OnPropertyChanged(() => CanViewDeleteAmbCardButton);
        }

        #region Actions

        private readonly DelegateCommand createNewPatientCommand;

        private readonly DelegateCommand saveChangesCommand;

        private readonly DelegateCommand cancelChangesCommand;

        private readonly DelegateCommand addRelativeCommand;

        private readonly DelegateCommand searchRelativeCommand;

        private readonly DelegateCommand createAmbCardCommand;

        private readonly DelegateCommand deleteAmbCardCommand;

        private readonly DelegateCommand printAmbCardCommand;

        private readonly DelegateCommand showAgreementsCommand;

        private readonly CommandWrapper saveChangesCommandWrapper;

        private readonly CommandWrapper loadRelativeListWrapper;

        private readonly CommandWrapper recreateAmbCardWrapper;

        private readonly CommandWrapper redeleteAmbCardWrapper;

        public ICommand CreateAmbCardCommand
        {
            get { return createAmbCardCommand; }
        }

        public ICommand DeleteAmbCardCommand
        {
            get { return deleteAmbCardCommand; }
        }

        public ICommand PrintAmbCardCommand
        {
            get { return printAmbCardCommand; }
        }

        public ICommand ShowAgreementsCommand
        {
            get { return showAgreementsCommand; }
        }

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
                selectedPatientOrRelative.NotificationMediator.Activate("Некоторые поля незаполнены или заполнены с ошибками", NotificationMediator.DefaultHideTime);
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
                    currentPatientId = PatientAssignmentListViewModel.PatientId = patientInfo.CurrentPerson.Id;
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
                UpdateAmbCardCommandsState();
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

        private async void AddRelativeAsync()
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

        private async void CreateAmbCardAsync()
        {
            log.InfoFormat("Creating amb card for person with id={0}", currentPatientId);
            try
            {
                var ambNumber = await patientService.CreateAmbCard(currentPatientId);
                if (ambNumber != string.Empty)
                    patientInfo.AmbNumber = ambNumber;
                UpdateAmbCardCommandsState();
                eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Publish(currentPatientId);
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to create amb card for person with id={0}", currentPatientId);
                FailureMediator.Activate("Не удалость завести амбулатоную карту. Попробуйте еще раз или обратитесь в службу поддержки", recreateAmbCardWrapper, ex, true);
            }
        }

        private async void DeleteAmbCardAsync()
        {
            log.InfoFormat("Deleting amb card for person with id={0}", currentPatientId);
            try
            {
                var res = await patientService.DeleteAmbCard(currentPatientId);
                if (res)
                    patientInfo.AmbNumber = string.Empty;
                UpdateAmbCardCommandsState();
                eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Publish(currentPatientId);
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to delete amb card for person with id={0}", currentPatientId);
                FailureMediator.Activate("Не удалость удалить амбулатоную карту. Попробуйте еще раз или обратитесь в службу поддержки", redeleteAmbCardWrapper, ex, true);
            }
        }

        private async void ShowAgreementsAsync()
        {
            if (SpecialValues.IsNewOrNonExisting(currentPatientId)) return;
            var agreementsViewModel = agreementsCollectionFactory();
            agreementsViewModel.LoadAgreementsAsync(currentPatientId);
            var result = await dialogService.ShowDialogAsync(agreementsViewModel);
        }

        private void PrintAmbCardAsync()
        {
            if (SpecialValues.IsNewOrNonExisting(currentPatientId)) return;
            var patient = patientService.GetPersonById(currentPatientId).First();
            if (string.IsNullOrEmpty(patient.AmbNumberString))
            {
                FailureMediator.Activate("Отсутствует номер А/К", true);
                return;
            }
            var report = reportGenerator.CreateDocX("AmbulatoryCard");
            string emptyValue = string.Empty;
            string defValue = "____________";
            string nonExistValue = "отсутствует";
            report.Data["NIKIName"] = recordService.GetDBSettingValue(DBSetting.NIKIName);
            report.Data["NIKIAddress"] = recordService.GetDBSettingValue(DBSetting.NIKIAddress);
            report.Data["OrgOKPO"] = recordService.GetDBSettingValue(DBSetting.OrgOKPO);

            report.Data["CardNumber"] = patient.AmbNumberString;
            report.Data["CardDate"] = patient.AmbNumberCreationDate.Value.ToShortDateString();
            report.Data["PatientFIO"] = patient.FullName;
            report.Data["BirthDate"] = patient.BirthDate.ToShortDateString();
            report.Data["Gender"] = patient.IsMale ? "муж. - 1" : "жен. - 2";
            if (patient.PersonAddresses.Any(x => x.AddressType.Options.Contains(OptionValues.AddressForAmbCard)))
            {
                var address = patient.PersonAddresses.Where(x => x.AddressType.Options.Contains(OptionValues.AddressForAmbCard)).OrderByDescending(x => x.BeginDateTime).First();
                report.Data["Address"] = address.UserText +
                                         (!string.IsNullOrEmpty(address.House) ? " д." + address.House : string.Empty) +
                                         (!string.IsNullOrEmpty(address.Building) ? " корп." + address.Building : string.Empty) +
                                         (!string.IsNullOrEmpty(address.Apartment) ? " кв." + address.Apartment : string.Empty);
                report.Data["Registration"] = address.UserText.ContainsAny(", г ", ", пгт ") ? "городская - 1" : "сельская – 2";
            }
            else
            {
                report.Data["Address"] = nonExistValue;
                report.Data["Registration"] = nonExistValue;
            }
            report.Data["ContactPhone"] = patient.Phones;
            report.Data["SNILS"] = patient.Snils;

            var omsDocument = patient.InsuranceDocuments.OrderByDescending(x => x.BeginDate).FirstOrDefault();
            if (omsDocument != null)
            {
                report.Data["InsuranceDocument"] = omsDocument.InsuranceDocumentType.Name + ": серия " + omsDocument.Series + " № " + omsDocument.Number;
                report.Data["InsuranceCompany"] = omsDocument.InsuranceCompany.NameSMOK;
            }
            else
            {
                report.Data["OMSDocument"] = nonExistValue;
                report.Data["InsuranceCompany"] = nonExistValue;                
            }          

            if (patient.PersonDisabilities.Any(x => !x.DisabilityType.IsDisability))
            {
                var benefit = patient.PersonDisabilities.Where(x => !x.DisabilityType.IsDisability).OrderByDescending(x => x.BeginDate).First();
                report.Data["BenefitCode"] = benefit.DisabilityType.BenefitCode;
                report.Data["BenefitDocument"] = benefit.DisabilityType.Name + " (серия " + benefit.Series + " № " + benefit.Number + ")"; 
            }   
            else
            {
                report.Data["BenefitCode"] = defValue;
                report.Data["BenefitDocument"] = defValue;
            }

            report.Data["MaritalStatus"] = patient.PersonMaritalStatuses.Any() ? patient.PersonMaritalStatuses.OrderByDescending(x => x.BeginDateTime).First().MaritalStatus.Name : defValue;
            report.Data["Education"] = patient.PersonEducations.Any() ? patient.PersonEducations.OrderByDescending(x => x.BeginDateTime).First().Education.Name : defValue;
            report.Data["Employment"] = patient.PersonSocialStatuses.Any() ? patient.PersonSocialStatuses.OrderByDescending(x => x.BeginDateTime).First().SocialStatusType.Name : defValue;
            if (patient.PersonSocialStatuses.Any(x => x.OrgId.HasValue))
            {
                var business = patient.PersonSocialStatuses.Where(x => x.OrgId.HasValue).OrderByDescending(x => x.BeginDateTime).First();
                report.Data["Business"] = business.Org.Name + (!string.IsNullOrEmpty(business.Office) + (", " + business.Office) + string.Empty);
            }
            else
                report.Data["Business"] = defValue;

            if (patient.PersonDisabilities.Any(x => x.DisabilityType.IsDisability))
            {
                var disability = patient.PersonDisabilities.Where(x => x.DisabilityType.IsDisability).OrderByDescending(x => x.BeginDate).First();
                report.Data["Disability"] = disability.DisabilityType.Name + " (серия " + disability.Series + " № " + disability.Number + ")";
            }
            else
                report.Data["Disability"] = defValue;

            report.Data["BloodGroup"] = defValue;
            report.Data["BloodRh"] = defValue;
            report.Data["Allergy"] = defValue;

            report.Data.Tables["hospdata"].AddRow(emptyValue, emptyValue, emptyValue);
            report.Data.Tables["radiationdata"].AddRow(emptyValue, emptyValue, emptyValue);
            report.Editable = false;
            report.Show();
        }

        private bool CanAddRelative()
        {
            return currentPatientId != SpecialValues.NonExistingId;
        }

        public bool CanViewCreateAmbCardButton { get { return currentPatientId > 0 && selectedPatientOrRelative == patientInfo && string.IsNullOrEmpty(patientInfo.AmbNumber); } }

        public bool CanViewDeleteAmbCardButton { get { return currentPatientId > 0 && selectedPatientOrRelative == patientInfo && !string.IsNullOrEmpty(patientInfo.AmbNumber); } }

        private bool CanCreateAmbCard()
        {
            return currentPatientId > 0 && selectedPatientOrRelative == patientInfo && securityService.HasPermission(Permission.CreateAmbCard);
        }

        private bool CanDeleteAmbCard()
        {
            return currentPatientId > 0 && !string.IsNullOrEmpty(patientInfo.AmbNumber) && selectedPatientOrRelative == patientInfo && securityService.HasPermission(Permission.DeleteAmbCard);
        }

        private bool CanPrintAmbCard()
        {
            return currentPatientId > 0 && !string.IsNullOrEmpty(patientInfo.AmbNumber) && selectedPatientOrRelative == patientInfo;
        }

        private bool CanShowAgreements()
        {
            return currentPatientId > 0 && selectedPatientOrRelative == patientInfo;
        }
        
        public ICommand SearchRelativeCommand
        {
            get { return searchRelativeCommand; }
        }

        private async void SearchRelativeAsync()
        {
            using (var searchViewModel = relativeSearchFactory())
            {
                var result = await dialogService.ShowDialogAsync(searchViewModel);
                if (result != true)
                {
                    return;
                }
                var foundRelativeId = searchViewModel.PersonSearchViewModel.SelectedPersonId;
                if (patientInfo.CurrentPerson != null && patientInfo.CurrentPerson.Id == foundRelativeId)
                {
                    SelectedPatientOrRelative.NotificationMediator.Activate("Пациент не может быть родственником самому себе", NotificationMediator.DefaultHideTime);
                    return;
                }
                if (Relatives.Any(x => x.CurrentPerson != null && x.CurrentPerson.Id == foundRelativeId))
                {
                    SelectedPatientOrRelative.NotificationMediator.Activate("У пациента уже есть этот родственник", NotificationMediator.DefaultHideTime);
                    return;
                }
                var newRelative = relativeInfoFactory();
                newRelative.IsRelative = true;
                newRelative.PersonRelative = new PersonRelative
                {
                    IsRepresentative = !Relatives.Any(x => x.IsRepresentative),
                    PersonId = patientInfo.CurrentPerson.Id,
                    RelativeId = foundRelativeId
                };
                Relatives.Add(newRelative);
                SelectedPatientOrRelative = newRelative;
                await newRelative.LoadPatientInfoAsync(foundRelativeId);

            }
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
                UpdateAmbCardCommandsState();
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
                currentPatientId = PatientAssignmentListViewModel.PatientId = targetPatientId;
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