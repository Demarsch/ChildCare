﻿using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Mvvm;
using log4net;
using Shared.PatientRecords.DTO;
using Shared.PatientRecords.Services;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Extensions;
using Core.Data.Services;
using System.Windows.Input;
using System.Data.Entity;
using Prism.Commands;
using Core.Wpf.Misc;
using Core.Misc;

namespace Shared.PatientRecords.ViewModels
{
    public class BrigadeViewModel : TrackableBindableBase, IChangeTrackerMediator, IDisposable
    {
        #region Fields
        private readonly IPatientRecordsService patientRecordsService;

        private readonly ILog logService;

        private CancellationTokenSource currentOperationToken;

        private BrigadeDTO brigadeDTO;

        #endregion

        #region Construcotrs
        public BrigadeViewModel(IPatientRecordsService patientRecordsService, ILog logService, BrigadeDTO brigadeDTO)
        {
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logSevice");
            }
            this.logService = logService;
            this.patientRecordsService = patientRecordsService;
            ChangeTracker = new ChangeTrackerEx<BrigadeViewModel>(this);
            RemoveRecordMemberCommand = new DelegateCommand(RemoveRecordMember);
            this.brigadeDTO = brigadeDTO;
            PersonStaffs = new ObservableCollectionEx<CommonIdName>();
            RecordTypeId = brigadeDTO.RecordTypeId;
            OnDate = brigadeDTO.OnDate;
            RoleId = brigadeDTO.RoleId;
            RecordTypeRolePermissionId = brigadeDTO.RecordTypeRolePermissionId;
            LoadPersonStaffsAsync(RecordTypeRolePermissionId);
            RoleName = brigadeDTO.RoleName + (brigadeDTO.IsRequired ? "*" : string.Empty);
            PermissionId = brigadeDTO.PermissionId;
            IsRequired = brigadeDTO.IsRequired;
            RecordMemberId = brigadeDTO.RecordMemberId;
            PersonName = brigadeDTO.PersonName;
            StaffName = brigadeDTO.StaffName;
            PersonStaffId = brigadeDTO.PersonStaffId;
            ChangeTracker.IsEnabled = true;
        }
        #endregion

        #region Properties
        private int recordTypeRolePermissionId;
        public int RecordTypeRolePermissionId
        {
            get { return recordTypeRolePermissionId; }
            set { SetProperty(ref recordTypeRolePermissionId, value); }
        }

        private int roleId;
        public int RoleId
        {
            get { return roleId; }
            set { SetProperty(ref roleId, value); }
        }

        private string roleName;
        public string RoleName
        {
            get { return roleName; }
            set { SetProperty(ref roleName, value); }
        }

        private int permissionId;
        public int PermissionId
        {
            get { return permissionId; }
            set { SetProperty(ref permissionId, value); }
        }

        private int recordMemberId;
        public int RecordMemberId
        {
            get { return recordMemberId; }
            set { SetProperty(ref recordMemberId, value); }
        }

        private bool isRequired;
        public bool IsRequired
        {
            get { return isRequired; }
            set { SetProperty(ref isRequired, value); }
        }

        private string personName;
        public string PersonName
        {
            get { return personName; }
            set { SetProperty(ref personName, value); }
        }

        private string staffName;
        public string StaffName
        {
            get { return staffName; }
            set { SetProperty(ref staffName, value); }
        }

        private int personStaffId;
        public int PersonStaffId
        {
            get { return personStaffId; }
            set
            {
                SetTrackedProperty(ref personStaffId, value);
                LoadPersonStaffDataAsync(PersonStaffId);
                OnPropertyChanged(() => IsPersonMember);
            }
        }

        private int recordTypeId;
        public int RecordTypeId
        {
            get { return recordTypeId; }
            set { SetProperty(ref recordTypeId, value); }
        }

        private DateTime onDate;
        public DateTime OnDate
        {
            get { return onDate; }
            set { SetProperty(ref onDate, value); }
        }

        public bool IsPersonMember { get { return PersonStaffId > 0; } }

        public ObservableCollectionEx<CommonIdName> PersonStaffs { get; set; }

        public IChangeTracker ChangeTracker { get; private set; }
        #endregion

        #region Methods

        private async void LoadPersonStaffsAsync(int permissionId)
        {
            //FailureMediator.Deactivate();
            if (permissionId < 1) return;
            var loadingIsCompleted = false;
            //BusyMediator.Activate(string.Empty);
            logService.InfoFormat("Loading PersonStaffs for permission with Id ={0}", permissionId);
            try
            {
                var personStaff = await Task.Run(() => patientRecordsService.GetAllowedPersonStaffs(RecordTypeId, RoleId, OnDate));
                PersonStaffs.AddRange(personStaff.ToList());
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load PersonStaffs for permission with Id ={0}", permissionId);
                //FailureMediator.Activate("Не удалось загрузить данные о сотрудниках, которым можно выполнять данную процедуру. Попробуйте еще раз или обратитесь в службу поддержки", reloadRecordBrigadeCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    //BusyMediator.Deactivate();
                }
            }
        }

        private async void LoadPersonStaffDataAsync(int personStaffId)
        {
            //FailureMediator.Deactivate();
            if (personStaffId < 1)
            {
                PersonName = string.Empty;
                StaffName = string.Empty;
                return;
            }
            var loadingIsCompleted = false;
            //BusyMediator.Activate(string.Empty);
            logService.InfoFormat("Loading PersonStaff data with Id ={0}", personStaffId);
            try
            {
                var personStaff = await patientRecordsService.GetPersonStaff(personStaffId).FirstOrDefaultAsync();
                PersonName = personStaff.PersonName;
                StaffName = personStaff.Staff.ShortName;
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load PersonStaff data with Id ={0}", personStaffId);
                //FailureMediator.Activate("Не удалось загрузить данные о сотрудниках, которым можно выполнять данную процедуру. Попробуйте еще раз или обратитесь в службу поддержки", reloadRecordBrigadeCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    //BusyMediator.Deactivate();
                }
            }
        }

        public RecordMember GetRecordMember()
        {
            return new RecordMember()
            {
                Id = brigadeDTO.RecordMemberId,
                IsActive = true,
                PersonStaffId = PersonStaffId,
                RecordTypeRolePermissionId = RecordTypeRolePermissionId,
                RecordId = brigadeDTO.RecordId
            };
        }

        public void Dispose()
        {
            ChangeTracker.Dispose();
        }
        #endregion

        #region Commands
        public ICommand RemoveRecordMemberCommand { get; private set; }
        private void RemoveRecordMember()
        {
            PersonStaffId = 0;
        }
        #endregion
    }
}
