using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using Shared.PatientRecords.DTO;
using Shared.PatientRecords.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Data.Entity;
using Core.Extensions;
using Core.Services;
using System.Threading;

namespace Shared.PatientRecords.ViewModels
{
    public class RecordCreateViewModel : BindableBase, IDialogViewModel
    {
        #region Fields
        private readonly RecordType unselectedRecordType;
        private readonly CommonIdName unselectedRoom;
        private readonly CommonIdName unselectedVisit;
        private readonly CommonIdName unselectedVisitTemplate;

        private readonly IPatientRecordsService patientRecordsService;
        private readonly IDialogService messageService;
        private readonly ILog logService;
        private readonly ISecurityService securityService;

        private readonly CommandWrapper recordTypeLoadingCommandWrapper;
        private readonly CommandWrapper recordCreatingCommandWrapper;

        private CancellationTokenSource currentOperationToken;

        private int personId = 0;
        #endregion


        #region Constructors
        public RecordCreateViewModel(IPatientRecordsService patientRecordsService, ILog logService, IDialogService messageService, ISecurityService securityService)
        {
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }
            if (securityService == null)
            {
                throw new ArgumentNullException("securityService");
            }
            this.securityService = securityService;
            this.messageService = messageService;
            this.patientRecordsService = patientRecordsService;
            this.logService = logService;
            recordTypeLoadingCommandWrapper = new CommandWrapper() { Command = new DelegateCommand<int?>(Initialize), CommandParameter = personId, CommandName = "Повторить" };
            recordCreatingCommandWrapper = new CommandWrapper() { Command = new DelegateCommand(CreateRecordAssignment) };
            unselectedRecordType = new RecordType { Name = "Выберите услугу", Assignable = false };
            unselectedRoom = new CommonIdName { Name = "Выберите кабинет", Id = -1 };
            unselectedVisit = new CommonIdName { Name = "Выберите случай", Id = -1 };
            unselectedVisitTemplate = new CommonIdName { Name = "Выберите шаблон случая", Id = -1 };
            CloseCommand = new DelegateCommand<bool?>(Close);
            FailureMediator = new FailureMediator();
            BusyMediator = new BusyMediator();
        }
        #endregion

        #region Properties
        private IEnumerable<RecordType> recordTypes;
        public IEnumerable<RecordType> RecordTypes
        {
            get { return recordTypes; }
            private set
            {
                if (SetProperty(ref recordTypes, value))
                {
                    SelectedRecordType = unselectedRecordType;
                }
            }
        }

        private CommonIdName[] rooms;
        public CommonIdName[] Rooms
        {
            get { return rooms; }
            private set
            {
                if (SetProperty(ref rooms, value))
                {
                    SelectedRoom = unselectedRoom;
                }
            }
        }

        private CommonIdName[] visits;
        public CommonIdName[] Visits
        {
            get { return visits; }
            private set
            {
                if (SetProperty(ref visits, value))
                {
                    SelectedVisit = unselectedVisit;
                }
            }
        }

        private CommonIdName[] visitTemplates;
        public CommonIdName[] VisitTemplates
        {
            get { return visitTemplates; }
            private set
            {
                if (SetProperty(ref visitTemplates, value))
                {
                    SelectedVisitTemplate = unselectedVisitTemplate;
                }
            }
        }

        private bool needCreateNewVisit;
        public bool NeedCreateNewVisit
        {
            get { return needCreateNewVisit; }
            set { SetProperty(ref needCreateNewVisit, value); }
        }

        private DateTime date;
        public DateTime Date
        {
            get { return date; }
            set { SetProperty(ref date, value); }
        }

        private RecordType selectedRecordType;
        public RecordType SelectedRecordType
        {
            get { return selectedRecordType; }
            set
            {
                value = value ?? unselectedRecordType;
                SetProperty(ref selectedRecordType, value);
            }
        }

        private CommonIdName selectedRoom;
        public CommonIdName SelectedRoom
        {
            get { return selectedRoom; }
            set
            {
                value = value ?? unselectedRoom;
                SetProperty(ref selectedRoom, value);
                //IsRecordTypeSelected = selectedRecordType != null && !ReferenceEquals(selectedRecordType, unselectedRecordType);
                //UpdateRoomFilter();
            }
        }

        private CommonIdName selectedVisit;
        public CommonIdName SelectedVisit
        {
            get { return selectedVisit; }
            set
            {
                value = value ?? unselectedVisit;
                SetProperty(ref selectedVisit, value);
            }
        }

        private CommonIdName selectedVisitTemplate;
        public CommonIdName SelectedVisitTemplate
        {
            get { return selectedVisitTemplate; }
            set
            {
                value = value ?? unselectedVisitTemplate;
                SetProperty(ref selectedVisitTemplate, value);
            }
        }

        public int RecordId { get; private set; }

        public BusyMediator BusyMediator { get; set; }

        public FailureMediator FailureMediator { get; private set; }
        #endregion

        #region Methods
        public async void Initialize(int? personId)
        {
            AssignIsSuccessful = false;
            RecordId = 0;
            if (personId.ToInt() < 1)
                return;
            selectedVisit = null;
            selectedVisitTemplate = null;
            selectedRoom = null;
            Date = DateTime.Now;
            NeedCreateNewVisit = false;
            logService.Info("Creating urgently record...");
            FailureMediator.Deactivate();
            BusyMediator.Activate("Создание услуги...");
            this.personId = personId.Value;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            DateTime dateTimeNow = DateTime.Now;
            IDisposableQueryable<Visit> recordVisitsQuery = null;
            try
            {
                // RecordTypes
                var newRecordTypes = await Task.Run((Func<IEnumerable<RecordType>>)patientRecordsService.GetRecordTypesToAssign);
                RecordTypes = new[] { unselectedRecordType }.Concat(newRecordTypes).ToArray();
                logService.InfoFormat("Loaded {0} record types", (RecordTypes as RecordType[]).Length);
                // Rooms
                var rooms = await Task.Factory.StartNew((Func<object, IEnumerable<Room>>)patientRecordsService.GetCashedRooms, dateTimeNow);
                Rooms = new[] { unselectedRoom }.Concat(rooms.Select(x => new CommonIdName() { Id = x.Id, Name = (string.IsNullOrEmpty(x.Number) ? x.Number + " - " : string.Empty) + x.Name })).OrderBy(x => x.Name).ToArray();
                logService.InfoFormat("Loaded {0} rooms", (Rooms as CommonIdName[]).Length);
                // Visits
                recordVisitsQuery = patientRecordsService.GetPersonVisitsQuery(this.personId, !securityService.HasPermission(Permission.UseCompletedVisit));
                var visits = await recordVisitsQuery.Select(x => new { x.Id, x.BeginDateTime, x.EndDateTime, x.VisitTemplate.Name }).OrderBy(x => x.BeginDateTime).ToArrayAsync(token);
                Visits = new[] { unselectedVisit }.Concat(visits.Select(x => new CommonIdName
                {
                    Id = x.Id,
                    Name = x.Name + " (" + x.BeginDateTime.ToString("dd.MM.yyyy HH:mm") + " - " +
                           (x.EndDateTime.HasValue ? x.EndDateTime.Value.ToString("dd.MM.yyyy HH:mm") : "...") + ")"
                })).ToArray();
                //VisitTemplates
                var visitTemplates = await Task.Factory.StartNew((Func<object, IEnumerable<VisitTemplate>>)patientRecordsService.GetCashedVisitTemplates, dateTimeNow);
                VisitTemplates = new[] { unselectedVisitTemplate }.Concat(visitTemplates.Select(x => new CommonIdName() { Id = x.Id, Name = x.Name })).OrderBy(x => x.Name).ToArray();
                logService.InfoFormat("Loaded {0} visit templatess", (VisitTemplates as CommonIdName[]).Length);
            }
            catch (Exception ex)
            {
                logService.Error("Failed to load datasources for schedule from database", ex);
                FailureMediator.Activate("При попытке загрузить список услуг возникла ошибка. Попробуйте перезапустить приложение. Если ошибка повторится, обратитесь в службу поддержки",
                                         recordTypeLoadingCommandWrapper,
                                         ex);
            }
            finally
            {
                if (recordVisitsQuery != null)
                {
                    recordVisitsQuery.Dispose();
                }
                BusyMediator.Deactivate();
            }
        }

        private async void CreateRecordAssignment()
        {
            logService.Info("Creating urgently record...");
            FailureMediator.Deactivate();
            BusyMediator.Activate("Создание записи...");
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            try
            {

                RecordId = 0;
                if (NeedCreateNewVisit)
                {
                    RecordId = await patientRecordsService.CreateUrgentRecord(personId, SelectedVisitTemplate.Id, SelectedRecordType.Id, SelectedRoom.Id, Date, token);
                }
                else
                {
                    RecordId = await patientRecordsService.CreateUrgentRecordInExistingVisit(personId, SelectedVisit.Id, SelectedRecordType.Id, SelectedRoom.Id, Date, token);
                }
                if (RecordId < 1)
                {
                    logService.Error("Failed to create urgently record");
                    FailureMediator.Activate("При попытке создать услугу возникла ошибка. Попробуйте еще раз, если ошибка повторится, обратитесь в службу поддержки",
                                         recordCreatingCommandWrapper,
                                         canBeDeactivated: true);
                }
                else
                {
                    logService.InfoFormat("Created urgently record with Id={0}", RecordId);
                    AssignIsSuccessful = true;
                    OnCloseRequested(new ReturnEventArgs<bool>(true));
                }
            }
            catch (Exception ex)
            {
                logService.Error("Failed to create urgently record", ex);
                FailureMediator.Activate("При попытке создать услугу возникла ошибка. Попробуйте еще раз, если ошибка повторится, обратитесь в службу поддержки",
                                         recordCreatingCommandWrapper,
                                         ex, true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }
        #endregion

        #region IDialogViewModel

        public string Title
        {
            get { return "Создать услугу"; }
        }

        public string ConfirmButtonText
        {
            get { return "Создать"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        public bool AssignIsSuccessful;

        private void Close(bool? validate)
        {
            //assignWasRequested = true;
            if (validate == true)
            {
                CreateRecordAssignment();
            }
            else
            {
                OnCloseRequested(new ReturnEventArgs<bool>(false));
            }
        }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(ReturnEventArgs<bool> e)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion
    }
}
