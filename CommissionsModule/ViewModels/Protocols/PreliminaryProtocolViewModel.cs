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
            CompositeChangeTracker = new ChangeTrackerEx<PreliminaryProtocolViewModel>(this);

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
                selectedCommissionTypeId = 0;
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
                selectedCommissionQuestionId = 0;
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
                selectedCommissionSourceId = 0;
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
                selectedSentLPUId = -2;
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
                selectedTalonId = 0;
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
                selectedPersonAddressId = 0;
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
                selectedHelpTypeId = 0;
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
        public IChangeTracker CompositeChangeTracker { get; private set; }

        public PersonSearchViewModel PersonSearchViewModel { get; set; }

        #endregion

        #region Commands
        private DelegateCommand selectMKBCommand;
        public ICommand SelectMKBCommand { get { return selectMKBCommand; } }

        #endregion

        #region Methods
        public bool GetPreliminaryCommissionProtocolData(ref CommissionProtocol commissionProtocol)
        {
            if (validationMediator.Validate())
            {
                commissionProtocol.CommissionTypeId = SelectedCommissionTypeId;
                commissionProtocol.CommissionQuestionId = SelectedCommissionQuestionId;
                commissionProtocol.CommissionSourceId = SelectedCommissionSourceId;
                commissionProtocol.SentLPUId = selectedSentLPUId == 0 ? (int?)null : selectedSentLPUId;
                commissionProtocol.PersonTalonId = !SpecialValues.IsNewOrNonExisting(SelectedTalonId) ? SelectedTalonId : (int?)null;
                commissionProtocol.MedicalHelpTypeId = !SpecialValues.IsNewOrNonExisting(SelectedHelpTypeId) ? SelectedHelpTypeId : (int?)null;
                commissionProtocol.MKB = MKB;
                commissionProtocol.IncomeDateTime = IncomeDateTime;
                commissionProtocol.PersonAddressId = SelectedPersonAddressId;
                return true;
            }
            return false;
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

        public async void Initialize(int commissionProtocolId = SpecialValues.NonExistingId, int personId = SpecialValues.NonExistingId)
        {
            CompositeChangeTracker.IsEnabled = false;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            CommissionProtocolId = commissionProtocolId;
            PersonId = personId;
            CommissionTypes.Clear();
            CommissionSources.Clear();
            CommissionQuestions.Clear();
            PersonAddresses.Clear();
            SentLPUs.Clear();
            HelpTypes.Clear();
            Talons.Clear();
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Загрузка протокола комиссии...");
            logService.InfoFormat("Loading commission protocol with id ={0} for person with id={1}", commissionProtocolId, personId);
            var commissionProtocolQuery = commissionService.GetCommissionProtocolById(commissionProtocolId);
            IDisposableQueryable<Org> commissionSentLPUquery = null;
            IDisposableQueryable<PersonTalon> talonQuery = null;
            IDisposableQueryable<PersonAddress> personAddressesQuery = null;
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
                // comboBox loading...
                commissionSentLPUquery = commissionService.GetCommissionSentLPUs(curDate);
                talonQuery = commissionService.GetPatientTalons(PersonId);
                personAddressesQuery = commissionService.GetPatientAddresses(PersonId);
                var commissionTypesTask = Task.Factory.StartNew((Func<object, IEnumerable<CommissionType>>)commissionService.GetCommissionTypes, curDate);
                var commissionSourcesTask = Task.Factory.StartNew((Func<object, IEnumerable<CommissionSource>>)commissionService.GetCommissionSource, curDate);
                var commissionQuestionsTask = Task.Factory.StartNew((Func<object, IEnumerable<CommissionQuestion>>)commissionService.GetCommissionQuestions, curDate);
                var commissionHelpTypesTask = Task.Factory.StartNew((Func<object, IEnumerable<MedicalHelpType>>)commissionService.GetCommissionMedicalHelpTypes, curDate);
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
                IEnumerable<CommissionType> commissionTypes = new CommissionType[] { unselectedCommissionType }.Concat(await commissionTypesTask);
                if (!token.IsCancellationRequested)
                    CommissionTypes.AddRange(commissionTypes);
                if (!token.IsCancellationRequested)
                    CommissionSources.AddRange(new CommissionSource[] { unselectedCommissionSource }.Concat(await commissionSourcesTask));
                if (!token.IsCancellationRequested)
                    CommissionQuestions.AddRange(new CommissionQuestion[] { unselectedCommissionQuestion }.Concat(await commissionQuestionsTask));
                var selfLPU = new CommonIdName { Id = 0, Name = "Самообращение" };
                if (!token.IsCancellationRequested)
                    SentLPUs.AddRange(new CommonIdName[] { unselectedSentLPU, selfLPU }.Concat(await commissionSentLPUsTask));
                if (!token.IsCancellationRequested)
                    HelpTypes.AddRange(new MedicalHelpType[] { unselectedHelpType }.Concat(await commissionHelpTypesTask));
                if (!token.IsCancellationRequested)
                    Talons.AddRange(new CommonIdName[] { unselectedTalon }.Concat((await talonQueryTask).Select(x => new CommonIdName
                    {
                        Id = x.Id,
                        Name = x.Name + " от " + x.date.ToShortDateString()
                    })));
                if (!token.IsCancellationRequested)
                    PersonAddresses.AddRange(new CommonIdName[] { unselectedPersonAddress }.Concat((await personAddressesTask).Select(x => new CommonIdName
                    {
                        Id = x.Id,
                        Name = x.Name + " ( действ. с " + x.BeginDateTime.ToShortDateString() + (x.EndDateTime != SpecialValues.MaxDate ? " по " + x.EndDateTime.ToShortDateString() : string.Empty) + ")"
                    })));

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
                CompositeChangeTracker.IsEnabled = true;
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
                    CompositeChangeTracker.IsEnabled = true;
                }
                if (commissionProtocolQuery != null)
                    commissionProtocolQuery.Dispose();
                if (commissionSentLPUquery != null)
                {
                    commissionSentLPUquery.Dispose();
                }
                if (talonQuery != null)
                {
                    talonQuery.Dispose();
                }
                if (personAddressesQuery != null)
                {
                    personAddressesQuery.Dispose();
                }
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
                SetError(x => x.SelectedPersonAddressId, SpecialValues.IsNewOrNonExisting(AssociatedItem.SelectedPersonAddressId) ? "Укажите адрес пациента" : string.Empty);
            }

            private void ValidateMKB()
            {
                SetError(x => x.MKB, string.IsNullOrEmpty(AssociatedItem.MKB) ? "Укажите диагноз по МКБ" : string.Empty);
            }

            private void ValidateSentLPU()
            {
                SetError(x => x.SelectedSentLPUId,AssociatedItem.SelectedSentLPUId == SpecialValues.NonExistingId ? "Укажите направившее ЛПУ" : string.Empty);
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
