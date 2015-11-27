using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Data.Misc;
using Core.Services;
using Core.Wpf.Mvvm;
using log4net;
using Prism.Mvvm;
using ScheduleModule.DTO;
using ScheduleModule.Services;

namespace ScheduleModule.ViewModels
{
    public class OverlayAssignmentCollectionViewModel : BindableBase
    {
        private readonly IScheduleService patientAssignmentService;

        private readonly ICacheService cacheService;

        private readonly ILog log;

        public OverlayAssignmentCollectionViewModel(IScheduleService patientAssignmentService, ICacheService cacheService, ILog log)
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
            Assignments = new ObservableCollectionEx<OverlayAssignmentViewModel>();
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            showCurrentPatientAssignments = false;
            currentPatient = SpecialValues.NonExistingId;
            //TODO: these values are used to avoid exception when TimelinePanel.Height becomes negative when bound to the same default DateTime value
            //TODO: probably worth using fallback value on binding side
            currentDateRoomsOpenTime = DateTime.Today.AddHours(8.0);
            currentDateRoomsCloseTime = DateTime.Today.AddHours(17.0);
        }

        public BusyMediator BusyMediator { get; private set; }

        public FailureMediator FailureMediator { get; private set; }

        private DateTime currentDateRoomsOpenTime;

        public DateTime CurrentDateRoomsOpenTime
        {
            get { return currentDateRoomsOpenTime; }
            set { SetProperty(ref currentDateRoomsOpenTime, value); }
        }

        private DateTime currentDateRoomsCloseTime;

        public DateTime CurrentDateRoomsCloseTime
        {
            get { return currentDateRoomsCloseTime; }
            set { SetProperty(ref currentDateRoomsCloseTime, value); }
        }

        private int currentPatient;

        public int CurrentPatient
        {
            get { return currentPatient; }
            set
            {
                var isChanged = SetProperty(ref currentPatient, value);
                CanShowCurrentPatientAssignments = value != SpecialValues.NonExistingId && value != SpecialValues.NewId;
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
                var isChanged = SetProperty(ref currentDate, value);
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
            set { SetProperty(ref showThatCurrentPatientHasNoAssignments, value); }
        }

        private bool canShowCurrentPatientAssignments;

        public bool CanShowCurrentPatientAssignments
        {
            get { return canShowCurrentPatientAssignments; }
            private set
            {
                var isChanged = SetProperty(ref canShowCurrentPatientAssignments, value);
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
                var isChanged = SetProperty(ref showCurrentPatientAssignments, value);
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

        public async void UpdateAssignmentAsync(int assignmentId)
        {
            try
            {
                if (currentPatient == SpecialValues.NonExistingId || currentPatient == SpecialValues.NewId)
                {
                    return;
                }
                var assignment = await Task<ScheduledAssignmentDTO>.Factory.StartNew(() => patientAssignmentService.GetAssignment(assignmentId, currentPatient));
                Assignments.RemoveWhere(x => x.Id == assignmentId);
                if (assignment != null && !assignment.IsCanceled)
                {
                    Assignments.Add(new OverlayAssignmentViewModel(assignment, cacheService));
                }
                ShowThatCurrentPatientHasNoAssignments = CanShowCurrentPatientAssignments && Assignments.Count == 0;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to update assignment (Id = {0})", assignmentId), ex);
                //No need to do anything else. This is more of informative procedure
            }
        }

        private async void LoadCurrentPatientAssignmentsAsync()
        {
            var currentPatientId = currentPatient;
            var currentDate = this.currentDate;
            log.InfoFormat("Loading overlay schedule assignments for patient (Id = {0}) for {1:dd.MM}", currentPatientId, currentDate);
            BusyMediator.Activate("Загрузка назначений...");
            FailureMediator.Deactivate();
            Assignments.Clear();
            ShowThatCurrentPatientHasNoAssignments = false;
            try
            {
                var assignments = await Task<IEnumerable<ScheduledAssignmentDTO>>.Factory.StartNew(() => patientAssignmentService.GetActualAssignments(currentPatientId, currentDate));
                if (currentPatient != currentPatientId)
                {
                    return;
                }
                Assignments.AddRange(assignments.Select(x => new OverlayAssignmentViewModel(x, cacheService)));
                ShowThatCurrentPatientHasNoAssignments = Assignments.Count == 0;
            }
            catch (Exception ex)
            {
                log.Error("Failed to load overlay schedule assignments", ex);
                FailureMediator.Activate("Не удалось загрузить назначения текущего пациента", exception: ex);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        public ObservableCollectionEx<OverlayAssignmentViewModel> Assignments { get; private set; }

    }
}
