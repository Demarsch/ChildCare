using Core.Data;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Shared.Patient.ViewModels;
using Shared.PatientRecords.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Extensions;
using CommissionsModule.Services;
using System.Threading;
using System.Data.Entity;
using Core.Data.Misc;
using System.ComponentModel;
using Core.Misc;

namespace CommissionsModule.ViewModels
{
    public class PreliminaryProtocolViewModel : TrackableBindableBase, IChangeTrackerMediator, IDataErrorInfo
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly IDialogServiceAsync dialogService;
        private readonly ILog logService;

        private CancellationTokenSource currentOperationToken;

        private readonly CommandWrapper reInitializeCommandWrapper;

        private readonly MKBTreeViewModel mkbTreeViewModel;

        private ValidationMediator validationMediator;

        private CommissionType unselectedCommissionType;
        private CommissionQuestion unselectedCommissionQuestion;
        private CommissionSource unselectedCommissionSource;
        private CommonIdName unselectedSentLPU;
        private CommonIdName unselectedTalon;
        private CommonIdName unselectedPersonAddress;
        private MedicalHelpType unselectedHelpType;

        private DateTime onDate;
        #endregion

        #region Constructors
        public PreliminaryProtocolViewModel(ICommissionService commissionService, IDialogServiceAsync dialogService, ILog logService, MKBTreeViewModel mkbTreeViewModel)
        {
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (mkbTreeViewModel == null)
            {
                throw new ArgumentNullException("mkbTreeViewModel");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }
            this.commissionService = commissionService;
            this.logService = logService;
            this.dialogService = dialogService;
            this.mkbTreeViewModel = mkbTreeViewModel;
            selectMKBCommand = new DelegateCommand(SelectMKB);
            reInitializeCommandWrapper = new CommandWrapper() { Command = new DelegateCommand(() => Initialize(CommissionProtocolId, PersonId)), CommandName = "Повторить" };

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            validationMediator = new ValidationMediator(this);
            ChangeTracker = new ChangeTrackerEx<PreliminaryProtocolViewModel>(this);

            CommissionQuestions = new ObservableCollectionEx<CommissionQuestion>();
            CommissionTypes = new ObservableCollectionEx<CommissionType>();
            CommissionSources = new ObservableCollectionEx<CommissionSource>();
            SentLPUs = new ObservableCollectionEx<CommonIdName>();
            Talons = new ObservableCollectionEx<CommonIdName>();
            PersonAddresses = new ObservableCollectionEx<CommonIdName>();
            HelpTypes = new ObservableCollectionEx<MedicalHelpType>();

            unselectedCommissionType = new CommissionType { Id = SpecialValues.NonExistingId, Name = "Выберите вид комиссии" };
            unselectedCommissionQuestion = new CommissionQuestion { Id = SpecialValues.NonExistingId, Name = "Выберите вопрос комиссии" };
            unselectedCommissionSource = new CommissionSource { Id = SpecialValues.NonExistingId, Name = "Выберите источник обращения" };
            unselectedSentLPU = new CommonIdName { Id = SpecialValues.NonExistingId, Name = "Выберите направившее ЛПУ/самообращение" };
            unselectedTalon = new CommonIdName { Id = SpecialValues.NonExistingId, Name = "Выберите талон пациента" };
            unselectedPersonAddress = new CommonIdName { Id = SpecialValues.NonExistingId, Name = "Выберите адрес пациента" };
            unselectedHelpType = new MedicalHelpType { Id = SpecialValues.NonExistingId, Name = "Выберите вид помощи" };
        }
        #endregion

        #region Properties

        public int CommissionProtocolId { get; private set; }
        public int PersonId { get; private set; }

        private int selectedCommissionTypeId;
        public int SelectedCommissionTypeId
        {
            get { return selectedCommissionTypeId; }
            set
            {
                if (value < 1)
                    value = SpecialValues.NonExistingId;
                SetTrackedProperty(ref selectedCommissionTypeId, value);
            }
        }

        private int selectedCommissionQuestionId;
        public int SelectedCommissionQuestionId
        {
            get { return selectedCommissionQuestionId; }
            set
            {
                if (value < 1)
                    value = SpecialValues.NonExistingId;
                SetTrackedProperty(ref selectedCommissionQuestionId, value);
            }
        }

        private int selectedCommissionSourceId;
        public int SelectedCommissionSourceId
        {
            get { return selectedCommissionSourceId; }
            set
            {
                if (value < 1)
                    value = SpecialValues.NonExistingId;
                SetTrackedProperty(ref selectedCommissionSourceId, value);
            }
        }

        private int selectedSentLPUId;
        public int SelectedSentLPUId
        {
            get { return selectedSentLPUId; }
            set
            {
                if (value < 0)
                    value = SpecialValues.NonExistingId;
                SetTrackedProperty(ref selectedSentLPUId, value);
            }
        }

        private int selectedTalonId;
        public int SelectedTalonId
        {
            get { return selectedTalonId; }
            set
            {
                if (value < 1)
                    value = SpecialValues.NonExistingId;
                SetTrackedProperty(ref selectedTalonId, value);
            }
        }

        private int selectedPersonAddressId;
        public int SelectedPersonAddressId
        {
            get { return selectedPersonAddressId; }
            set
            {
                if (value < 1)
                    value = SpecialValues.NonExistingId;
                SetTrackedProperty(ref selectedPersonAddressId, value);
            }
        }

        private int selectedHelpTypeId;
        public int SelectedHelpTypeId
        {
            get { return selectedHelpTypeId; }
            set
            {
                if (value < 1)
                    value = SpecialValues.NonExistingId;
                SetTrackedProperty(ref selectedHelpTypeId, value);
            }
        }

        private DateTime incomeDateTime;
        public DateTime IncomeDateTime
        {
            get { return incomeDateTime; }
            set { SetTrackedProperty(ref incomeDateTime, value); }
        }

        private string mkb;
        public string MKB
        {
            get { return mkb; }
            set { SetTrackedProperty(ref mkb, value); }
        }

        private string patient;
        public string Patient
        {
            get { return patient; }
            set { SetProperty(ref patient, value); }
        }

        //DataSources
        public ObservableCollectionEx<CommissionType> CommissionTypes { get; private set; }
        public ObservableCollectionEx<CommissionQuestion> CommissionQuestions { get; private set; }
        public ObservableCollectionEx<CommissionSource> CommissionSources { get; private set; }
        public ObservableCollectionEx<CommonIdName> SentLPUs { get; private set; }
        public ObservableCollectionEx<CommonIdName> Talons { get; private set; }
        public ObservableCollectionEx<CommonIdName> PersonAddresses { get; private set; }
        public ObservableCollectionEx<MedicalHelpType> HelpTypes { get; private set; }

        public BusyMediator BusyMediator { get; private set; }
        public FailureMediator FailureMediator { get; private set; }
        public IChangeTracker ChangeTracker { get; private set; }

        public PersonSearchViewModel PersonSearchViewModel { get; set; }

        #endregion

        #region Commands
        private DelegateCommand selectMKBCommand;
        public ICommand SelectMKBCommand { get { return selectMKBCommand; } }

        #endregion

        #region Methods
        public void GetPreliminaryCommissionProtocolData(ref CommissionProtocol commissionProtocol)
        {
            commissionProtocol.CommissionTypeId = SelectedCommissionTypeId;
            commissionProtocol.CommissionQuestionId = SelectedCommissionQuestionId;
            commissionProtocol.CommissionSourceId = SelectedCommissionSourceId;
            commissionProtocol.SentLPUId = selectedSentLPUId == 0 ? (int?)null : selectedSentLPUId;
            commissionProtocol.PersonTalonId = SelectedTalonId;
            commissionProtocol.MedicalHelpTypeId = SelectedHelpTypeId;
            commissionProtocol.MKB = MKB;
            commissionProtocol.IncomeDateTime = IncomeDateTime;
            commissionProtocol.PersonAddressId = SelectedPersonAddressId;
        }

        public async void SelectMKB()
        {
            var result = await dialogService.ShowDialogAsync(mkbTreeViewModel);
            if (result == true)
            {
                var selectedMKB = mkbTreeViewModel.MKBTree.Any(x => x.IsSelected) ? mkbTreeViewModel.MKBTree.First(x => x.IsSelected) : mkbTreeViewModel.MKBTree.SelectMany(x => x.Children).First(x => x.IsSelected);
                MKB = selectedMKB.Code;
            }
        }

        private async Task<bool> LoadDataSources(DateTime onDate, int personId, CancellationToken token)
        {
            CommissionTypes.Clear();
            CommissionSources.Clear();
            CommissionQuestions.Clear();
            PersonAddresses.Clear();
            SentLPUs.Clear();
            HelpTypes.Clear();
            Talons.Clear();
            BusyMediator.Activate("Загрузка справочников...");
            logService.InfoFormat("Loading data sources for PreliminaryProtocol for commission protocol with id={0}", CommissionProtocolId);
            this.onDate = onDate;
            var commissionSentLPUquery = commissionService.GetCommissionSentLPUs(onDate);
            var talonQuery = commissionService.GetPatientTalons(personId);
            var personAddressesQuery = commissionService.GetPatientAddresses(personId);
            var dataloaded = false;
            try
            {
                var commissionTypesTask = Task.Factory.StartNew((Func<object, IEnumerable<CommissionType>>)commissionService.GetCommissionTypes, onDate);
                var commissionSourcesTask = Task.Factory.StartNew((Func<object, IEnumerable<CommissionSource>>)commissionService.GetCommissionSource, onDate);
                var commissionQuestionsTask = Task.Factory.StartNew((Func<object, IEnumerable<CommissionQuestion>>)commissionService.GetCommissionQuestions, onDate);
                var commissionSentLPUsTask = commissionSentLPUquery.Select(x => new CommonIdName { Id = x.Id, Name = x.Name }).ToArrayAsync();
                var talonQueryTask = talonQuery.Select(x => new
                {
                    Id = x.Id,
                    Name = x.TalonNumber + ":" + x.MedicalHelpType.ShortName,
                    date = x.TalonDateTime
                }).ToArrayAsync();
                var personAddressesTask = personAddressesQuery.Select(x => new
                {
                    x.Id,
                    Name = x.AddressType.Name + ": " + x.UserText,
                    x.BeginDateTime,
                    x.EndDateTime
                }).OrderBy(x => x.BeginDateTime).ToArrayAsync();
                var commissionHelpTypesTask = Task.Factory.StartNew((Func<object, IEnumerable<MedicalHelpType>>)commissionService.GetCommissionMedicalHelpTypes, onDate);
                await Task.WhenAll(commissionTypesTask, commissionSourcesTask, commissionQuestionsTask, commissionSentLPUsTask, commissionHelpTypesTask, talonQueryTask, personAddressesTask);
                IEnumerable<CommissionType> commissionTypes = new CommissionType[] { unselectedCommissionType }.Concat(commissionTypesTask.Result);
                if (!token.IsCancellationRequested)
                    CommissionTypes.AddRange(commissionTypes);
                if (!token.IsCancellationRequested)
                    CommissionSources.AddRange(new CommissionSource[] { unselectedCommissionSource }.Concat(commissionSourcesTask.Result));
                if (!token.IsCancellationRequested)
                    CommissionQuestions.AddRange(new CommissionQuestion[] { unselectedCommissionQuestion }.Concat(commissionQuestionsTask.Result));
                var selfLPU = new CommonIdName { Id = 0, Name = "Самообращение" };
                if (!token.IsCancellationRequested)
                    SentLPUs.AddRange(new CommonIdName[] { unselectedSentLPU, selfLPU }.Concat(commissionSentLPUsTask.Result));
                if (!token.IsCancellationRequested)
                    HelpTypes.AddRange(new MedicalHelpType[] { unselectedHelpType }.Concat(commissionHelpTypesTask.Result));
                if (!token.IsCancellationRequested)
                    Talons.AddRange(new CommonIdName[] { unselectedTalon }.Concat(talonQueryTask.Result.Select(x => new CommonIdName
                    {
                        Id = x.Id,
                        Name = x.Name + " от " + x.date.ToShortDateString()
                    })));
                if (!token.IsCancellationRequested)
                    PersonAddresses.AddRange(new CommonIdName[] { unselectedPersonAddress }.Concat(personAddressesTask.Result.Select(x => new CommonIdName
                    {
                        Id = x.Id,
                        Name = x.Name + " ( действ. с " + x.BeginDateTime.ToShortDateString() + (x.EndDateTime != SpecialValues.MaxDate ? " по " + x.EndDateTime.ToShortDateString() : string.Empty) + ")"
                    })));
                logService.InfoFormat("Loaded data sources  for PreliminaryProtocol for commission protocol with id={0}", CommissionProtocolId);
                dataloaded = true;
                this.onDate = DateTime.MaxValue;
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources  for PreliminaryProtocol for commission protocol with id={0}", CommissionProtocolId);
                dataloaded = false;
            }
            finally
            {
                if (commissionSentLPUquery != null)
                {
                    commissionSentLPUquery.Dispose();
                }
                if (talonQuery != null)
                {
                    talonQuery.Dispose();
                }
                BusyMediator.Deactivate();
            }
            return dataloaded;
        }

        public async void Initialize(int commissionProtocolId = SpecialValues.NonExistingId, int personId = SpecialValues.NonExistingId)
        {
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            CommissionProtocolId = commissionProtocolId;
            PersonId = personId;
            SelectedCommissionTypeId = SpecialValues.NonExistingId;
            SelectedCommissionQuestionId = SpecialValues.NonExistingId;
            SelectedCommissionSourceId = SpecialValues.NonExistingId;
            MKB = string.Empty;
            SelectedSentLPUId = SpecialValues.NonExistingId;
            SelectedTalonId = SpecialValues.NonExistingId;
            SelectedPersonAddressId = SpecialValues.NonExistingId;
            SelectedHelpTypeId = SpecialValues.NonExistingId;
            IncomeDateTime = DateTime.Now;

            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Загрузка протокола комиссии...");
            logService.InfoFormat("Loading commission protocol with id ={0} for person with id={1}", commissionProtocolId, personId);
            var commissionProtocolQuery = commissionService.GetCommissionProtocolById(commissionProtocolId);
            var curDate = DateTime.Now;
            try
            {
                var commissionProtocolData = await commissionProtocolQuery.Select(x => new
                {
                    x.CommissionTypeId,
                    x.CommissionQuestionId,
                    x.CommissionSourceId,
                    x.MKB,
                    x.SentLPUId,
                    x.PersonTalonId,
                    x.MedicalHelpTypeId,
                    x.IncomeDateTime,
                    x.PersonId,
                    x.PersonAddressId
                }).FirstOrDefaultAsync(token);
                if (commissionProtocolData != null)
                {
                    PersonId = commissionProtocolData.PersonId;
                    curDate = commissionProtocolData.IncomeDateTime;
                }
                var res = await LoadDataSources(curDate, PersonId, token);
                if (!res)
                {
                    logService.ErrorFormatEx(null, "Failed to load commission data sources with commission id ={0} for person with id={1}", commissionProtocolId, personId);
                    FailureMediator.Activate("Не удалость загрузить справочники. Попробуйте еще раз или обратитесь в службу поддержки", reInitializeCommandWrapper);
                    return;
                }

                if (commissionProtocolData != null)
                {
                    SelectedCommissionTypeId = commissionProtocolData.CommissionTypeId;
                    SelectedCommissionQuestionId = commissionProtocolData.CommissionQuestionId;
                    SelectedCommissionSourceId = commissionProtocolData.CommissionSourceId;
                    MKB = commissionProtocolData.MKB;
                    SelectedSentLPUId = commissionProtocolData.SentLPUId.ToInt();
                    SelectedTalonId = commissionProtocolData.PersonTalonId.ToInt();
                    SelectedPersonAddressId = commissionProtocolData.PersonAddressId.ToInt();
                    SelectedHelpTypeId = commissionProtocolData.MedicalHelpTypeId.ToInt();
                    IncomeDateTime = commissionProtocolData.IncomeDateTime;
                }
                else
                {
                    SelectedCommissionTypeId = SpecialValues.NonExistingId;
                    SelectedCommissionQuestionId = SpecialValues.NonExistingId;
                    SelectedCommissionSourceId = SpecialValues.NonExistingId;
                    MKB = string.Empty;
                    SelectedSentLPUId = SpecialValues.NonExistingId;
                    SelectedTalonId = SpecialValues.NonExistingId;
                    SelectedPersonAddressId = SpecialValues.NonExistingId;
                    SelectedHelpTypeId = SpecialValues.NonExistingId;
                    IncomeDateTime = DateTime.Now;
                }
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load commission protocol with id ={0} for person with id={1}", commissionProtocolId, personId);
                FailureMediator.Activate("Не удалость загрузить протокол комиссии. Попробуйте еще раз или обратитесь в службу поддержки", reInitializeCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                    ChangeTracker.IsEnabled = true;
                }
                if (commissionProtocolQuery != null)
                    commissionProtocolQuery.Dispose();
            }
        }
        #endregion

        #region IDataErrorInfo implementation
        public string Error
        {
            get { return validationMediator.Error; }
        }

        public string this[string columnName]
        {
            get { return validationMediator[columnName]; }
        }

        public bool Validate()
        {
            return validationMediator.Validate();
        }

        public void CancelValidation()
        {
            validationMediator.CancelValidation();
        }

        public class ValidationMediator : ValidationMediator<PreliminaryProtocolViewModel>
        {

            public ValidationMediator(PreliminaryProtocolViewModel associatedItem)
                : base(associatedItem)
            {
            }

            protected override void OnValidateProperty(string propertyName)
            {
                if (PropertyNameEquals(propertyName, x => x.SelectedCommissionTypeId))
                {
                    ValidateCommissionType();
                }
                else if (PropertyNameEquals(propertyName, x => x.SelectedCommissionQuestionId))
                {
                    ValidateCommissionQuestion();
                }
                else if (PropertyNameEquals(propertyName, x => x.SelectedCommissionSourceId))
                {
                    ValidateCommissionSource();
                }
                else if (PropertyNameEquals(propertyName, x => x.SelectedSentLPUId))
                {
                    ValidateSentLPU();
                }
                else if (PropertyNameEquals(propertyName, x => x.MKB))
                {
                    ValidateMKB();
                }
                else if (PropertyNameEquals(propertyName, x => x.SelectedPersonAddressId))
                {
                    ValidatePersonAddress();
                }
            }

            private void ValidatePersonAddress()
            {
                SetError(x => x.SelectedSentLPUId, SpecialValues.IsNewOrNonExisting(AssociatedItem.SelectedPersonAddressId) ? "Укажите адрес пациента" : string.Empty);
            }

            private void ValidateMKB()
            {
                SetError(x => x.MKB, string.IsNullOrEmpty(AssociatedItem.MKB) ? "Укажите диагноз по МКБ" : string.Empty);
            }

            private void ValidateSentLPU()
            {
                SetError(x => x.SelectedSentLPUId, SpecialValues.IsNewOrNonExisting(AssociatedItem.SelectedSentLPUId) ? "Укажите направившее ЛПУ" : string.Empty);
            }

            private void ValidateCommissionSource()
            {
                SetError(x => x.SelectedCommissionSourceId, SpecialValues.IsNewOrNonExisting(AssociatedItem.SelectedCommissionSourceId) ? "Укажите источник обращения" : string.Empty);
            }

            private void ValidateCommissionQuestion()
            {
                SetError(x => x.SelectedCommissionQuestionId, SpecialValues.IsNewOrNonExisting(AssociatedItem.SelectedCommissionQuestionId) ? "Укажите рассматриваемый комиссией вопрос" : string.Empty);
            }

            private void ValidateCommissionType()
            {
                SetError(x => x.SelectedCommissionTypeId, SpecialValues.IsNewOrNonExisting(AssociatedItem.SelectedCommissionTypeId) ? "Укажите вид комиссии" : string.Empty);
            }

            protected override void RaiseAssociatedObjectPropertyChanged()
            {
                AssociatedItem.OnPropertyChanged(string.Empty);
            }

            protected override void OnValidate()
            {
                ValidateCommissionType();
                ValidateCommissionQuestion();
                ValidateCommissionSource();
                ValidateSentLPU();
                ValidateMKB();
                ValidatePersonAddress();
            }
        }

        #endregion
    }
}
