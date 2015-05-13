using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Commission
{
    [PropertyChanged.ImplementPropertyChanged]
    public class CommissionNavigatorViewModel : ViewModelBase
    {
        public CommissionNavigatorViewModel(CommissionService commissionService, UserService userService, PersonService personService)
        {
            NavigationItems = new ObservableCollection<dynamic>();
            NavigationCommand = new RelayCommand(NavigationAction);
            this.commissionService = commissionService;
            this.userService = userService;
            this.personService = personService;
            myCommissions = new ObservableCollection<CommissionProtocol>();
            Load();
        }

        // Used
        public ObservableCollection<dynamic> NavigationItems { get; set; }
        public dynamic SelectedItem { get; set; }
        public event EventHandler<dynamic> Navigated;
        public ICommand NavigationCommand { get; set; }
        private ObservableCollection<CommissionProtocol> myCommissions;
        private ICommissionService commissionService;
        private IUserService userService;
        private IPersonService personService;

        void NavigationAction()
        {
            if (SelectedItem != null && Navigated != null) Navigated(this, SelectedItem);
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
            if (e.ProgressPercentage == 0) 
                NavigationItems.Clear();
            NavigationItems.Add(e.UserState);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            myCommissions.Clear();
            int percent = 0;

            foreach (var commission in commissionService.GetCommissionsByUserId(userService.GetCurrentUser().Id))
            { 
                myCommissions.Add(commission);
                var person = personService.GetPersonById(commission.PersonId);
                worker.ReportProgress(++percent, new CommissionProtocolDTO() { Id = commission.Id,
                                                                               PatientFIO = person.ShortName,
                                                                               BirthDate = person.BirthYear,
                                                                               Talon = commission.PersonTalonId.HasValue ? personService.GetPersonTalonById(commission.PersonTalonId.Value).TalonNumber : string.Empty,
                                                                               MKB = "МКБ:" + commission.MKB,
                                                                               IncomeDateTime = " с " + commission.IncomeDateTime.ToShortDateString() });
            }

            /*worker.ReportProgress(0, new { Id = 1, Name = "1111" });
            worker.ReportProgress(1, new { Id = 2, Name = "2222" });
            worker.ReportProgress(2, new { Id = 3, Name = "3333" });*/
        }
    }
}