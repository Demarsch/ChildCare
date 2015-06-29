using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Core;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;

namespace Registry
{
    public class PatientAssignmentListViewModel : BasicViewModel
    {
        private readonly IPatientAssignmentService assignmentService;

        private readonly ILog log;

        private readonly ICacheService cacheService;

        private ObservableCollection<PatientAssignmentViewModel> assignments;

        public ObservableCollection<PatientAssignmentViewModel> Assignments
        {
            get { return assignments; }
            set { Set("Assignments", ref assignments, value); }
        }

        public PatientAssignmentListViewModel(IPatientAssignmentService assignmentService, ILog log, ICacheService cacheService)
        {
            if (assignmentService == null)
                throw new ArgumentNullException("assignmentService");
            if (log == null)
                throw new ArgumentNullException("log");
            if (cacheService == null)
                throw new ArgumentNullException("cacheService");
            this.cacheService = cacheService;
            this.log = log;
            this.assignmentService = assignmentService;
            assignments = new ObservableCollection<PatientAssignmentViewModel>();
            LoadCommand = new RelayCommand(Load, CanLoad);
            showIncompleted = true;
        }

        public ICommand LoadCommand { get; private set; }

        private int patientId;

        public int PatientId
        {
            get { return patientId; }
            set
            {
                var isPatientSelected = IsPatientSelected;
                Assignments.Clear();
                IsLoaded = false;
                IsLoadingRequested = false;
                FailReason = string.Empty;
                patientId = value;
                if (isPatientSelected != IsPatientSelected)
                {
                    RaisePropertyChanged("IsPatientSelected");
                    CommandManager.InvalidateRequerySuggested();
                };
            }
        }

        public bool IsPatientSelected { get { return patientId != 0; } }

        private bool showIncompleted;

        public bool ShowIncompleted
        {
            get { return showIncompleted; }
            set
            {
                var isChanged = Set("ShowIncompleted", ref showIncompleted, value);
                if (!(showCancelled || showCompleted || showIncompleted))
                    isChanged = Set("ShowCompleted", ref showCompleted, true);
                if (isChanged)
                    RefreshAssignments();
            }
        }

        private bool showCompleted;

        public bool ShowCompleted
        {
            get { return showCompleted; }
            set
            {
                var isChanged = Set("ShowCompleted", ref showCompleted, value);
                if (!(showCancelled || showCompleted || showIncompleted))
                    isChanged = Set("ShowCancelled", ref showCancelled, true);
                if (isChanged)
                    RefreshAssignments();
            }
        }

        private bool showCancelled;

        public bool ShowCancelled
        {
            get { return showCancelled; }
            set
            {
                var isChanged = Set("ShowCancelled", ref showCancelled, value);
                if (!(showCancelled || showCompleted || showIncompleted))
                    isChanged = Set("ShowIncompleted", ref showIncompleted, true);
                if (isChanged)
                    RefreshAssignments();
            }
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
            FailReason = string.Empty;
            Assignments.Clear();
            IsLoading = true;
            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task<ICollection<PatientAssignmentViewModel>>.Factory.StartNew(LoadImpl, patientId)
                .ContinueWith(LoadCompleted, uiScheduler);
        }

        private bool CanLoad()
        {
            return !IsLoading && IsPatientSelected && !IsLoaded;
        }

        private ICollection<PatientAssignmentViewModel> LoadImpl(object patientId)
        {
            return assignmentService.GetAssignments((int)patientId).Select(x => new PatientAssignmentViewModel(x, cacheService)).ToArray();
        }

        private void LoadCompleted(Task<ICollection<PatientAssignmentViewModel>> sourceTask)
        {
            var newSearchWasExecuted = patientId != (int)sourceTask.AsyncState;
            try
            {
                var result = sourceTask.Result;
                if (newSearchWasExecuted)
                    return;
                var newAssignments = new ObservableCollection<PatientAssignmentViewModel>(result);
                UpdateDefaultView(newAssignments);
                Assignments = newAssignments;
            }
            catch (AggregateException ex)
            {
                var innerException = ex.InnerExceptions[0];
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

        private void UpdateDefaultView(ObservableCollection<PatientAssignmentViewModel> sourceCollection)
        {
            var defaultView = CollectionViewSource.GetDefaultView(sourceCollection);
            defaultView.SortDescriptions.Add(new SortDescription("AssignDateTime", ListSortDirection.Descending));
            defaultView.Filter = FilterAssignments;

        }

        private bool FilterAssignments(object obj)
        {
            var assignment = obj as PatientAssignmentViewModel;
            return assignment != null && ((assignment.State == AssignmentState.Cancelled && showCancelled)
                                        || (assignment.State == AssignmentState.Completed && showCompleted)
                                        || ((assignment.State == AssignmentState.Incompleted || assignment.State == AssignmentState.Temporary) && showIncompleted));
        }

        private void RefreshAssignments()
        {
            CollectionViewSource.GetDefaultView(assignments).Refresh();
        }
    }
}
