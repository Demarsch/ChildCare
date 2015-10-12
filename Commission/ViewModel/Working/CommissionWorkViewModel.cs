using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MainLib.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Commission
{
    [PropertyChanged.ImplementPropertyChanged]
    public class CommissionWorkViewModel : ViewModelBase
    {
        private ICommissionService commissionService;
        private IUserService userService;
        private IUserSystemInfoService userSystemInfoService;
        private IPersonService personService;
        public CommissionDecisionViewModel Decision { get; set; }
        public PersonDocumentsViewModel PersonDocuments { get; set; }

        public CommissionWorkViewModel(ICommissionService commissionService, IUserService userService, IUserSystemInfoService userSystemInfoService, IPersonService personService,
                                       IDocumentService documentService, CommissionDecisionViewModel decision, PersonDocumentsViewModel personDocuments)
        {
            Decision = decision;
            PersonDocuments = personDocuments;
            this.commissionService = commissionService;
            this.userService = userService;
            this.personService = personService;
            this.userSystemInfoService = userSystemInfoService;

            NavigationItems = new ObservableCollection<CommissionProtocolDTO>();
            NavigationCommand = new RelayCommand(NavigationAction);
            Load();
        }

        public ObservableCollection<CommissionProtocolDTO> NavigationItems { get; set; }
        public CommissionProtocolDTO SelectedItem { get; set; }
        public ICommand NavigationCommand { get; set; }

        void NavigationAction()
        {
            if (SelectedItem != null)
            {
                Decision.Load(SelectedItem.Id);
                PersonDocuments.Load(SelectedItem.PersonId);
            }
        }

        BackgroundWorker worker;
        public void Load()
        {
            if (worker == null)
            {
                worker = new BackgroundWorker();
                worker.Setup();
                worker.DoWork += worker_DoWork;
                worker.ProgressChanged += worker_ProgressChanged;
                worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            }
            worker.WaitForRestart();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState == null) 
                NavigationItems.Clear();
            else
                NavigationItems.Add(e.UserState as CommissionProtocolDTO);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // clear list
            worker.ReportProgress(0);
            int percent = 0;
            foreach (var commission in commissionService.GetCommissionsByMemberPersonId(userService.GetCurrentUser(userSystemInfoService).PersonId, false))
            {
                //cancelation
                if (worker.CancellationPending)
                {
                    percent = 0;
                    return;
                }

                var person = personService.GetPersonById(commission.PersonId);
                worker.ReportProgress(++percent, new CommissionProtocolDTO()
                {
                    Id = commission.Id,
                    PersonId = commission.PersonId,
                    PatientFIO = person.ShortName,
                    BirthDate = person.BirthYear,
                    Talon = commission.PersonTalonId.HasValue ? "Талон: " + personService.GetPersonTalonById(commission.PersonTalonId.Value).TalonNumber : "(талона нет)",
                    MKB = (!string.IsNullOrWhiteSpace(commission.MKB) ? "МКБ: " + commission.MKB : string.Empty),
                    IncomeDateTime = " направлен с " + commission.IncomeDateTime.ToShortDateString()
                });
            }
        }
    }
}