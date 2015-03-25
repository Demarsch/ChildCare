using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Core;
using GalaSoft.MvvmLight.Command;
using log4net;

namespace Registry
{
    public class PatientAssignmentListViewModel : FailableViewModel
    {
        private readonly IPatientAssignmentService assignmentService;

        private readonly ILog log;

        private ObservableCollection<AssignmentViewModel> assignments;

        public ObservableCollection<AssignmentViewModel> Assignments
        {
            get { return assignments; }
            set { Set("Assignments", ref assignments, value); }
        }

        public PatientAssignmentListViewModel(IPatientAssignmentService assignmentService, ILog log)
        {
            if (assignmentService == null)
                throw new ArgumentNullException("assignmentService");
            if (log == null)
                throw new ArgumentNullException("log");
            this.log = log;
            this.assignmentService = assignmentService;
            assignments = new ObservableCollection<AssignmentViewModel>();
            LoadCommand = new RelayCommand(Load, CanLoad);
        }

        public ICommand LoadCommand { get; private set; }

        private int patientId;

        public int PatientId
        {
            get { return patientId; }
            set
            {
                Assignments.Clear();
                IsLoaded = false;
                IsLoadingRequested = false;
                FailReason = string.Empty;
                patientId = value;
            }
        }

        private bool showCompleted;

        public bool ShowCompleted
        {
            get { return showCompleted; }
            set { Set("ShowCompleted", ref showCompleted, value); }
        }

        private bool showCancelled;

        public bool ShowCancelled
        {
            get { return showCancelled; }
            set { Set("ShowCancelled", ref showCancelled, value); }
        }

        private bool isLoaded;

        public bool IsLoaded
        {
            get { return isLoaded; }
            set { Set("IsLoaded", ref isLoaded, value); }
        }

        private bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                if (Set("IsLoading", ref isLoading, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool isLoadingRequested;

        public bool IsLoadingRequested
        {
            get { return isLoadingRequested; }
            set
            {
                if (Set("IsLoadingRequested", ref isLoadingRequested, value) && value && LoadCommand.CanExecute(null))
                    LoadCommand.Execute(null);
            }
        }

        private void Load()
        {
            IsLoaded = false;
            IsLoading = true;
            FailReason = string.Empty;
            Assignments.Clear();
            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task<ICollection<AssignmentViewModel>>.Factory.StartNew(LoadImpl, patientId)
                .ContinueWith(LoadCompleted, uiScheduler);
        }

        private bool CanLoad()
        {
            return !IsLoading;
        }

        private ICollection<AssignmentViewModel> LoadImpl(object patientId)
        {
            return assignmentService.GetAssignments((int)patientId).Select(x => new AssignmentViewModel(x)).ToArray();
        }

        private void LoadCompleted(Task<ICollection<AssignmentViewModel>> sourceTask)
        {
            var newSearchWasExecuted = patientId != (int)sourceTask.AsyncState;
            try
            {
                var result = sourceTask.Result;
                if (newSearchWasExecuted)
                    return;
                Assignments = new ObservableCollection<AssignmentViewModel>(result);
            }
            catch (AggregateException ex)
            {
                var innerException = ex.InnerExceptions[0];
                //TODO: probably move this string to separate localizable dll
                FailReason = "При попытке загрузить наначения возникла ошибка. Возможно отсутствует связь с базой данной. Попробуйте обновить список. Если ошибка повторится, обратитесь в службу поддержки";
                log.Error(string.Format("Failed to load assignment for patient Id of '{0}'", sourceTask.AsyncState), innerException);
            }
            finally
            {
                if (!newSearchWasExecuted)
                {
                    IsLoading = false;
                    IsLoaded = true;
                }
            }
        }
    }
}
