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

namespace CommissionsModule.ViewModels
{
    public class PreliminaryProtocolViewModel : TrackableBindableBase
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly IDialogServiceAsync dialogService;
        private readonly ILog logService;

        private CancellationTokenSource currentOperationToken;

        private readonly CommandWrapper reInitializeCommandWrapper;

        private readonly MKBTreeViewModel mkbTreeViewModel;

        private DateTime onDate;
        #endregion

        #region Constructors
        public PreliminaryProtocolViewModel(ICommissionService commissionService, IDialogServiceAsync dialogService, ILog logService, MKBTreeViewModel mkbTreeViewModel, AddressViewModel addressViewModel)
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
            AddressViewModel = addressViewModel;
            reInitializeCommandWrapper = new CommandWrapper() { Command = new DelegateCommand(() => Initialize(CommissionProtocolId)), CommandName = "Повторить" };

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();

            CommissionQuestions = new ObservableCollectionEx<CommissionQuestion>();
            CommissionTypes = new ObservableCollectionEx<CommissionType>();
            CommissionSources = new ObservableCollectionEx<CommissionSource>();
            SentLPUs = new ObservableCollectionEx<Org>();
            Talons = new ObservableCollectionEx<PersonTalon>();
            HelpTypes = new ObservableCollectionEx<MedicalHelpType>();
        }
        #endregion

        #region Properties

        public int CommissionProtocolId { get; private set; }

        private int selectedCommissionTypeId;
        public int SelectedCommissionTypeId
        {
            get { return selectedCommissionTypeId; }
            set { SetTrackedProperty(ref selectedCommissionTypeId, value); }
        }

        private int selectedCommissionQuestionId;
        public int SelectedCommissionQuestionId
        {
            get { return selectedCommissionQuestionId; }
            set { SetTrackedProperty(ref selectedCommissionQuestionId, value); }
        }

        private int selectedCommissionSourceId;
        public int SelectedCommissionSourceId
        {
            get { return selectedCommissionSourceId; }
            set { SetTrackedProperty(ref selectedCommissionSourceId, value); }
        }

        private int selectedSentLPUId;
        public int SelectedSentLPUId
        {
            get { return selectedSentLPUId; }
            set { SetTrackedProperty(ref selectedSentLPUId, value); }
        }

        private int selectedTalonId;
        public int SelectedTalonId
        {
            get { return selectedTalonId; }
            set { SetTrackedProperty(ref selectedTalonId, value); }
        }

        private int selectedHelpTypeId;
        public int SelectedHelpTypeId
        {
            get { return selectedHelpTypeId; }
            set { SetTrackedProperty(ref selectedHelpTypeId, value); }
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

        public AddressViewModel AddressViewModel { get; private set; }

        //DataSources
        public ObservableCollectionEx<CommissionType> CommissionTypes { get; private set; }
        public ObservableCollectionEx<CommissionQuestion> CommissionQuestions { get; private set; }
        public ObservableCollectionEx<CommissionSource> CommissionSources { get; private set; }
        public ObservableCollectionEx<Org> SentLPUs { get; private set; }
        public ObservableCollectionEx<PersonTalon> Talons { get; private set; }
        public ObservableCollectionEx<MedicalHelpType> HelpTypes { get; private set; }

        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; set; }
        public PersonSearchViewModel PersonSearchViewModel { get; set; }

        #endregion

        #region Commands
        private DelegateCommand selectMKBCommand;
        public ICommand SelectMKBCommand { get { return selectMKBCommand; } }
        #endregion

        #region Methods
        public async void SelectMKB()
        {
            var result = await dialogService.ShowDialogAsync(mkbTreeViewModel);
            if (result == true)
            {
                var selectedMKB = mkbTreeViewModel.MKBTree.Any(x => x.IsSelected) ? mkbTreeViewModel.MKBTree.First(x => x.IsSelected) : mkbTreeViewModel.MKBTree.SelectMany(x => x.Children).First(x => x.IsSelected);
                MKB = selectedMKB.Code;
            }
        }

        private async Task<bool> LoadDataSources(DateTime onDate, int personId)
        {
            CommissionTypes.Clear();
            CommissionSources.Clear();
            CommissionQuestions.Clear();
            SentLPUs.Clear();
            HelpTypes.Clear();
            Talons.Clear();
            BusyMediator.Activate("Загрузка справочников...");
            logService.InfoFormat("Loading data sources for PreliminaryProtocol for commission protocol with id={0}", CommissionProtocolId);
            this.onDate = onDate;
            var commissionSentLPUquery = commissionService.GetCommissionSentLPUs(onDate);
            var talonQuery = commissionService.GetPatientTalons(personId);
            var dataloaded = false;
            try
            {
                var commissionTypesTask = Task.Factory.StartNew((Func<object, IEnumerable<CommissionType>>)commissionService.GetCommissionType, onDate);
                var commissionSourcesTask = Task.Factory.StartNew((Func<object, IEnumerable<CommissionSource>>)commissionService.GetCommissionSource, onDate);
                var commissionQuestionsTask = Task.Factory.StartNew((Func<object, IEnumerable<CommissionQuestion>>)commissionService.GetCommissionQuestions, onDate);
                var commissionSentLPUsTask = commissionSentLPUquery.ToArrayAsync();
                var talonQueryTask = talonQuery.ToArrayAsync();
                var commissionHelpTypesTask = Task.Factory.StartNew((Func<object, IEnumerable<MedicalHelpType>>)commissionService.GetCommissionMedicalHelpTypes, onDate);
                await Task.WhenAll(commissionTypesTask, commissionSourcesTask, commissionQuestionsTask, commissionSentLPUsTask, commissionHelpTypesTask, talonQueryTask);
                CommissionTypes.AddRange(commissionTypesTask.Result);
                CommissionSources.AddRange(commissionSourcesTask.Result);
                CommissionQuestions.AddRange(commissionQuestionsTask.Result);
                SentLPUs.AddRange(commissionSentLPUsTask.Result);
                HelpTypes.AddRange(commissionHelpTypesTask.Result);
                Talons.AddRange(talonQueryTask.Result);
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
                BusyMediator.Deactivate();
            }
            return dataloaded;
        }

        public async void Initialize(int commissionProtocolId = SpecialValues.NonExistingId, int personId = SpecialValues.NonExistingId)
        {
            CommissionProtocolId = commissionProtocolId;
            SelectedCommissionTypeId = -1;
            SelectedCommissionQuestionId = -1;
            SelectedCommissionSourceId = -1;
            MKB = string.Empty;
            SelectedSentLPUId = -1;
            SelectedTalonId = -1;
            SelectedHelpTypeId = -1;
            IncomeDateTime = DateTime.Now;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Загрузка протокола комиссии...");
            logService.InfoFormat("Loading commission protocol with id ={0}", commissionProtocolId);
            var commissionProtocolQuery = commissionService.GetCommissionProtocolById(commissionProtocolId);
            var curDate = DateTime.Now;
            var persId = personId;
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
                    x.PersonId
                }).FirstOrDefaultAsync(token);
                if (commissionProtocolData != null)
                {
                    persId = commissionProtocolData.PersonId;
                    curDate = commissionProtocolData.IncomeDateTime;
                }
                var res = await LoadDataSources(curDate, persId);
                if (!res)
                {
                    logService.ErrorFormatEx(null, "Failed to load commission data sources with commission id ={0}", commissionProtocolId);
                    FailureMediator.Activate("Не удалость загрузить справочники. Попробуйте еще раз или обратитесь в службу поддержки", reInitializeCommandWrapper);
                    return;
                }

                if (commissionProtocolData != null)
                {
                    SelectedCommissionTypeId = commissionProtocolData.CommissionTypeId;
                    SelectedCommissionQuestionId = commissionProtocolData.CommissionQuestionId;
                    SelectedCommissionSourceId = commissionProtocolData.CommissionSourceId;
                    MKB = commissionProtocolData.MKB;
                    SelectedSentLPUId = commissionProtocolData.SentLPUId;
                    SelectedTalonId = commissionProtocolData.PersonTalonId.ToInt();
                    SelectedHelpTypeId = commissionProtocolData.MedicalHelpTypeId.ToInt();
                    IncomeDateTime = commissionProtocolData.IncomeDateTime;
                }
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load commission protocol with id ={0}", commissionProtocolId);
                FailureMediator.Activate("Не удалость загрузить протокол комиссии. Попробуйте еще раз или обратитесь в службу поддержки", reInitializeCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                if (loadingIsCompleted)
                    BusyMediator.Deactivate();
                if (commissionProtocolQuery != null)
                    commissionProtocolQuery.Dispose();
            }
        }
        #endregion
    }
}
