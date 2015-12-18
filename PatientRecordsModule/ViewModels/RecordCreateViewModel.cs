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

namespace Shared.PatientRecords.ViewModels
{
    public class RecordCreateViewModel : BindableBase, IDialogViewModel
    {
        #region Fields
        private readonly RecordType unselectedRecordType;
        private readonly CommonIdName unselectedRoom;

        private readonly IPatientRecordsService patientRecordsService;
        private readonly IDialogService messageService;
        private readonly ILog logService;

        private readonly CommandWrapper recordTypeLoadingCommandWrapper;

        private int personId = 0;
        #endregion


        #region Constructors
        public RecordCreateViewModel(IPatientRecordsService patientRecordsService, ILog logService, IDialogService messageService)
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
            this.messageService = messageService;
            this.patientRecordsService = patientRecordsService;
            this.logService = logService;
            recordTypeLoadingCommandWrapper = new CommandWrapper() { Command = new DelegateCommand<int?>(Initialize), CommandParameter = personId };
            unselectedRecordType = new RecordType { Name = "Выберите услугу", Assignable = false };
            unselectedRoom = new CommonIdName { Name = "Выберите кабинет", Id = -1 };
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

        private List<CommonIdName> rooms;
        public List<CommonIdName> Rooms
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

        private RecordType selectedRecordType;
        public RecordType SelectedRecordType
        {
            get { return selectedRecordType; }
            set
            {
                value = value ?? unselectedRecordType;
                SetProperty(ref selectedRecordType, value);
                //IsRecordTypeSelected = selectedRecordType != null && !ReferenceEquals(selectedRecordType, unselectedRecordType);
                //UpdateRoomFilter();
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

        public BusyMediator BusyMediator { get; set; }

        public FailureMediator FailureMediator { get; private set; }
        #endregion

        #region Methods
        public async void Initialize(int? personId)
        {
            if (personId.ToInt() < 1)
                return;
            this.personId = personId.Value;
            logService.Info("Creating urgently record...");
            FailureMediator.Deactivate();
            BusyMediator.Activate("Создание услуги...");
            try
            {
                var newRecordTypes = await Task.Run((Func<IEnumerable<RecordType>>)patientRecordsService.GetRecordTypesToAssign);
                RecordTypes = new[] { unselectedRecordType }.Concat(newRecordTypes).ToArray();
                logService.InfoFormat("Loaded {0} record types", (RecordTypes as RecordType[]).Length);
                SelectedRecordType = unselectedRecordType;
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
                BusyMediator.Deactivate();
            }
        }

        private async void CreateRecordAssignment()
        {
            logService.Info("Loading recordTypes and rooms for record creating...");
            FailureMediator.Deactivate();
            BusyMediator.Activate("Загрузка данных...");
            IDisposableQueryable<Room> roomsQuery = null;
            try
            {
                var newRecordTypes = await Task.Run((Func<IEnumerable<RecordType>>)patientRecordsService.GetRecordTypesToAssign);
                RecordTypes = new[] { unselectedRecordType }.Concat(newRecordTypes).ToArray();
                logService.InfoFormat("Loaded {0} record types", (RecordTypes as RecordType[]).Length);
                SelectedRecordType = unselectedRecordType;

                roomsQuery = patientRecordsService.GetRooms(DateTime.Now);
                var rooms = await roomsQuery.Select(x => new CommonIdName
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();
                Rooms.AddRange(rooms);
                logService.InfoFormat("Loaded {0} rooms", rooms.Count);
            }
            catch (Exception ex)
            {
                logService.Error("Failed to load recordTypes and rooms for record creating", ex);
                FailureMediator.Activate("При попытке загрузить список услуг и кабинетов возникла ошибка. Попробуйте перезапустить приложение. Если ошибка повторится, обратитесь в службу поддержки",
                                         recordTypeLoadingCommandWrapper,
                                         ex);
            }
            finally
            {
                if (roomsQuery != null)
                {
                    roomsQuery.Dispose();
                }
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
