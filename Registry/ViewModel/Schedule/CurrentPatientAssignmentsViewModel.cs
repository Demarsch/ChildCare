using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using log4net;

namespace Registry
{
    public class CurrentPatientAssignmentsViewModel : BasicViewModel
    {
        private readonly IPatientAssignmentService patientAssignmentService;

        private readonly ICacheService cacheService;

        private readonly ILog log;

        public CurrentPatientAssignmentsViewModel(IPatientAssignmentService patientAssignmentService, ICacheService cacheService, ILog log)
        {
            if (patientAssignmentService == null)
            {
                throw new ArgumentNullException("patientAssignmentService");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            this.log = log;
            this.cacheService = cacheService;
            this.patientAssignmentService = patientAssignmentService;
            Assignments = new ObservalbeCollectionEx<CurrentPatientAssignmentViewModel>();
            showCurrentPatientAssignments = false;
            //TODO: these values are used to avoid exception when TimelinePanel.Height becomes negative when bound to the same default DateTime value
            //TODO: probably worth using fallback value on binding side
            currentDateRoomsOpenTime = DateTime.Today.AddHours(8.0);
            currentDateRoomsCloseTime = DateTime.Today.AddHours(17.0);
        }

        private DateTime currentDateRoomsOpenTime;

        public DateTime CurrentDateRoomsOpenTime
        {
            get { return currentDateRoomsOpenTime; }
            set { Set("CurrentDateRoomsOpenTime", ref currentDateRoomsOpenTime, value); }
        }

        private DateTime currentDateRoomsCloseTime;

        public DateTime CurrentDateRoomsCloseTime
        {
            get { return currentDateRoomsCloseTime; }
            set { Set("CurrentDateRoomsCloseTime", ref currentDateRoomsCloseTime, value); }
        }

        private PersonViewModel currentPatient;

        public PersonViewModel CurrentPatient
        {
            get { return currentPatient; }
            set
            {
                var isChanged = Set("CurrentPatient", ref currentPatient, value);
                CanShowCurrentPatientAssignments = value != null;
                if (isChanged && ShowCurrentPatientAssignments)
                {
                    LoadCurrentPatientAssignmentsAsync();
                }
            }
        }

        private DateTime currentDate;

        public DateTime CurrentDate
        {
            get { return currentDate; }
            set
            {
                var isChanged = Set("CurrentDate", ref currentDate, value);
                if (isChanged && ShowCurrentPatientAssignments)
                {
                    LoadCurrentPatientAssignmentsAsync();
                }
            }
        }

        private bool showThatCurrentPatientHasNoAssignments;

        public bool ShowThatCurrentPatientHasNoAssignments
        {
            get { return showThatCurrentPatientHasNoAssignments; }
            set { Set("ShowThatCurrentPatientHasNoAssignments", ref showThatCurrentPatientHasNoAssignments, value); }
        }

        private bool canShowCurrentPatientAssignments;

        public bool CanShowCurrentPatientAssignments
        {
            get { return canShowCurrentPatientAssignments; }
            private set
            {
                var isChanged = Set("CanShowCurrentPatientAssignments", ref canShowCurrentPatientAssignments, value);
                if (isChanged && !value)
                {
                    ShowCurrentPatientAssignments = false;
                }
            }
        }

        private bool showCurrentPatientAssignments;

        public bool ShowCurrentPatientAssignments
        {
            get { return showCurrentPatientAssignments; }
            set
            {
                var isChanged = Set("ShowCurrentPatientAssignments", ref showCurrentPatientAssignments, value);
                if (!isChanged)
                {
                    return;
                }
                if (!value)
                {
                    ShowThatCurrentPatientHasNoAssignments = false;
                    Assignments.Clear();
                }
                else
                {
                    LoadCurrentPatientAssignmentsAsync();
                }
            }
        }

        public async Task UpdateAssignmentAsync(int assignmentId)
        {
            try
            {
                if (currentPatient == null)
                {
                    return;
                }
                var assignment = await Task<AssignmentScheduleDTO>.Factory.StartNew(() => patientAssignmentService.GetAssignment(assignmentId, currentPatient.Id));
                Assignments.RemoveWhere(x => x.Id == assignmentId);
                if (assignment != null && !assignment.IsCanceled)
                {
                    Assignments.Add(new CurrentPatientAssignmentViewModel(assignment, cacheService));
                }
                ShowThatCurrentPatientHasNoAssignments = CanShowCurrentPatientAssignments && Assignments.Count == 0;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to update assignment (Id = {0})", assignmentId), ex);
                //No need to do anything else. This is more of informative procedure
            }
        }

        private async Task LoadCurrentPatientAssignmentsAsync()
        {
            var currentPatientId = currentPatient.Id;
            var currentDate = this.currentDate;
            log.InfoFormat("Loading overlay schedule assignments for patient (Id = {0}) for {1:dd.MM}", currentPatientId, currentDate);
            BusyStatus = "Загрузка назначений...";
            FailReason = null;
            Assignments.Clear();
            ShowThatCurrentPatientHasNoAssignments = false;
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            if (currentPatient.Id != currentPatientId)
            {
                return;
            }
            try
            {
                var assignments = await Task<ICollection<AssignmentScheduleDTO>>.Factory.StartNew(() => patientAssignmentService.GetActualAssignments(currentPatientId, currentDate));
                if (currentPatient.Id != currentPatientId)
                {
                    return;
                }
                Assignments.AddRange(assignments.Select(x => new CurrentPatientAssignmentViewModel(x, cacheService)));
                ShowThatCurrentPatientHasNoAssignments = Assignments.Count == 0;
            }
            catch (Exception ex)
            {
                log.Error("Failed to load overlay schedule assignments", ex);
                FailReason = "Не удалось загрузить назначения текущего пациента";
            }
            finally
            {
                BusyStatus = null;
            }
        }

        public ObservalbeCollectionEx<CurrentPatientAssignmentViewModel> Assignments { get; private set; }

    }
}
