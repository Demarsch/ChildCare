using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using Core.Data;
using Core.Data.Misc;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Mvvm;
using log4net;
using Prism.Mvvm;
using Shared.Patient.Services;

namespace Shared.Patient.ViewModels
{
    public class PatientAssignmentListViewModel : BindableBase
    {
        private readonly IPatientAssignmentService patientAssignmentService;

        private readonly ILog log;

        private readonly ICacheService cacheService;

        public PatientAssignmentListViewModel(IPatientAssignmentService patientAssignmentService, ILog log, ICacheService cacheService)
        {
            if (patientAssignmentService == null)
            {
                throw new ArgumentNullException("patientAssignmentService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.patientAssignmentService = patientAssignmentService;
            this.log = log;
            this.cacheService = cacheService;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            Assignments = new ObservableCollectionEx<PatientAssignmentViewModel>();
        }

        public ObservableCollectionEx<PatientAssignmentViewModel> Assignments { get; private set; }

        public BusyMediator BusyMediator { get; private set; }

        public FailureMediator FailureMediator { get; private set; }

        private int patientId;

        public int PatientId
        {
            get { return patientId; }
            set
            {
                if (SetProperty(ref patientId, value))
                {
                    PatientIsSelected = !value.IsNewOrNonExisting();
                    NoAssignments = true;
                }
            }
        }

        private bool patientIsSelected;

        public bool PatientIsSelected
        {
            get { return patientIsSelected; }
            private set { SetProperty(ref patientIsSelected, value); }
        }

        private int launchSequencee;

        private bool startAssignmentLoading;

        public bool StartAssignmentLoading
        {
            get { return startAssignmentLoading; }
            set
            {
                if (SetProperty(ref startAssignmentLoading, value) && value && !PatientId.IsNewOrNonExisting())
                {
                    LoadAssignmentsAsync(PatientId, Interlocked.Increment(ref launchSequencee));
                }
            }
        }

        private bool noAssignments;

        public bool NoAssignments
        {
            get { return noAssignments; }
            private set { SetProperty(ref noAssignments, value); }
        }

        private async void LoadAssignmentsAsync(int patientId, int currentLaunchSequence)
        {
            FailureMediator.Deactivate();
            BusyMediator.Activate("Загрузка списка...");
            NoAssignments = false;
            try
            {
                var assignments = await patientAssignmentService.GetAssignmentsQuery(patientId)
                                                                .Where(x => !x.IsTemporary && x.CancelDateTime == null)
                                                                .Select(x => new
                                                                             {
                                                                                 x.Id,
                                                                                 x.AssignDateTime,
                                                                                 x.RoomId,
                                                                                 x.RecordTypeId,
                                                                                 IsCompleted = x.RecordId != null && x.Record.IsCompleted
                                                                             })
                                                                .ToArrayAsync();
                if (launchSequencee == currentLaunchSequence)
                {
                    Assignments.Replace(assignments.Select(x => new PatientAssignmentViewModel
                                                                {
                                                                    AssignDateTime = x.AssignDateTime,
                                                                    Id = x.Id,
                                                                    IsCompleted = x.IsCompleted,
                                                                    RecordType = cacheService.GetItemById<RecordType>(x.RecordTypeId).Name,
                                                                    Room = cacheService.GetItemById<Room>(x.RoomId).Name
                                                                }));
                }

            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to load assignment list for patient with Id {0}", patientId);
                FailureMediator.Activate("Не удалось загрузить список назначений", null, ex, true);
            }
            finally
            {
                if (launchSequencee == currentLaunchSequence)
                {
                    NoAssignments = Assignments.Count == 0;
                    BusyMediator.Deactivate();
                }
            }
        }
    }
}
