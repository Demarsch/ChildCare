using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Commission
{
    [PropertyChanged.ImplementPropertyChanged]
    public class CommissionNavigatorViewModel : ViewModelBase
    {
        public CommissionNavigatorViewModel()
        {
            NavigationItems = new ObservableCollection<dynamic>();
            NavigationCommand = new RelayCommand(NavigationAction);

            Load();
        }

        // Used
        public ObservableCollection<dynamic> NavigationItems { get; set; }
        public dynamic SelectedItem { get; set; }
        public event EventHandler<dynamic> Navigated;
        public ICommand NavigationCommand { get; set; } 
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
            if (e.ProgressPercentage == 0) NavigationItems.Clear();
            NavigationItems.Add(e.UserState);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            worker.ReportProgress(0, new { Id = 1, Name = "1111" });
            worker.ReportProgress(1, new { Id = 2, Name = "2222" });
            worker.ReportProgress(2, new { Id = 3, Name = "3333" });
        }
    }
}