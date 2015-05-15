using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
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
        private IPersonService personService;
        public CommissionDecisionViewModel Decision { get; set; }

        public CommissionWorkViewModel(CommissionService commissionService, UserService userService, PersonService personService,
            CommissionDecisionViewModel decision)
        {
            Decision = decision;
            this.commissionService = commissionService;
            this.userService = userService;
            this.personService = personService;

            NavigationItems = new ObservableCollection<CommissionProtocolDTO>();
            NavigationCommand = new RelayCommand(NavigationAction);
            Load();
        }

        public ObservableCollection<CommissionProtocolDTO> NavigationItems { get; set; }
        public CommissionProtocolDTO SelectedItem { get; set; }
        public ICommand NavigationCommand { get; set; }

        void NavigationAction()
        {
            if (SelectedItem != null) Decision.Load(SelectedItem.Id);
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

            foreach (var commission in commissionService.GetCommissionsByUserId(userService.GetCurrentUser().Id))
            {
                //cancelation
                if (worker.CancellationPending) return;

                var person = personService.GetPersonById(commission.PersonId);
                worker.ReportProgress(1, new CommissionProtocolDTO()
                {
                    Id = commission.Id,
                    PatientFIO = person.ShortName,
                    BirthDate = person.BirthYear,
                    Talon = commission.PersonTalonId.HasValue ? personService.GetPersonTalonById(commission.PersonTalonId.Value).TalonNumber : string.Empty,
                    MKB = "МКБ:" + commission.MKB,
                    IncomeDateTime = " с " + commission.IncomeDateTime.ToShortDateString()
                });
            }
        }
    }
}