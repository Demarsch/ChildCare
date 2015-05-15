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
    public class CommissionManagementViewModel : ViewModelBase
    {
        public CommissionPersonGridViewModel CommissionGrid { get; set; }
        public ICommissionService commissionService;

        public CommissionManagementViewModel(CommissionPersonGridViewModel CommissionGrid, ICommissionService commissionService)
        {
            this.CommissionGrid = CommissionGrid;
            this.commissionService = commissionService;

            NavigationItems = new ObservableCollection<dynamic>();
            NavigationCommand = new RelayCommand(NavigationAction);
        }

        public ObservableCollection<dynamic> NavigationItems { get; set; }
        public dynamic SelectedItem { get; set; }
        public ICommand NavigationCommand { get; set; }

        void NavigationAction()
        {
            if (SelectedItem != null && CommissionGrid != null) CommissionGrid.Load(SelectedItem);
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
                NavigationItems.Add(e.UserState);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // clearlist
            worker.ReportProgress(0);

            var filter = new CommissionServiceFilter()
            {
                IsActive = true
            };

            foreach (var commission in commissionService.GetCommissionsByFilter(filter))
            {
                //var person = personService.GetPersonById(commission.PersonId);
                worker.ReportProgress(1, commission);
            }
        }
    }
}