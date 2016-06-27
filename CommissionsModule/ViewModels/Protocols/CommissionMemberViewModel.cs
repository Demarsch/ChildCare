using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Navigation;
using Core.Data;
using Core.Data.Classes;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;
using Shared.Patient.Misc;
using Core.Misc;
using CommissionsModule.Services;
using System.Windows.Input;
using System.Collections.Specialized;
using CommissionsModule.ViewModels.Protocols;

namespace CommissionsModule.ViewModels
{
    public class CommissionMemberViewModel : BindableBase, IDataErrorInfo
    {
        private readonly ICommissionService commissionService;
        private readonly ILog logService;

        private CancellationTokenSource cancellationToken;

        public CommissionMemberViewModel(ICommissionService commissionService, ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }
            this.logService = logService;
            this.commissionService = commissionService;

            ContextMenuItems = new ObservableCollectionEx<CommissionMemberStageViewModel>();
            BusyMediator = new BusyMediator();
            IsChanged = true;
        }

        #region Properties

        public int Id { get; set; }
        public string StaffName { get; set; }
        public string PersonName { get; set; }
        public string MemberTypeName { get; set; }


        public ObservableCollectionEx<CommissionMemberStageViewModel> contextMenuItems;
        public ObservableCollectionEx<CommissionMemberStageViewModel> ContextMenuItems
        {
            get { return contextMenuItems; }
            private set
            {
                SetProperty(ref contextMenuItems, value);
            }
        }

        public BusyMediator BusyMediator { get; set; }

        private bool isChanged;
        public bool IsChanged
        {
            get { return isChanged; }
            set { SetProperty(ref isChanged, value); }
        }

        private DateTime beginDateTime;
        public DateTime BeginDateTime
        {
            get { return beginDateTime; }
            set
            {
                if (SetProperty(ref beginDateTime, value))
                {
                    IsChanged = true;
                };
            }
        }

        private DateTime endDateTime;
        public DateTime EndDateTime
        {
            get { return endDateTime; }
            set
            {
                if (SetProperty(ref endDateTime, value))
                {
                    IsChanged = true;
                };
            }
        }

        private ObservableCollectionEx<FieldValue> memberTypes;
        public ObservableCollectionEx<FieldValue> MemberTypes
        {
            get
            {
                if (memberTypes == null)
                    memberTypes = new ObservableCollectionEx<FieldValue>();
                return memberTypes;
            }
            set { SetProperty(ref memberTypes, value); }
        }

        private int selectedMemberTypeId;
        public int SelectedMemberTypeId
        {
            get { return selectedMemberTypeId; }
            set
            {
                if (SetProperty(ref selectedMemberTypeId, value))
                {
                    IsChanged = true;
                };
            }
        }

        private ObservableCollectionEx<FieldValue> personStaffs;
        public ObservableCollectionEx<FieldValue> PersonStaffs
        {
            get
            {
                if (personStaffs == null)
                    personStaffs = new ObservableCollectionEx<FieldValue>();
                return personStaffs;
            }
            set { SetProperty(ref personStaffs, value); }
        }

        private int selectedPersonStaffId;
        public int SelectedPersonStaffId
        {
            get { return selectedPersonStaffId; }
            set
            {
                if (SetProperty(ref selectedPersonStaffId, value))
                {
                    SelectedStaffId = SpecialValues.NonExistingId;
                    CanSelectStaff = (value == SpecialValues.NonExistingId);
                    OnPropertyChanged(() => SelectedStaffId);
                    IsChanged = true;
                }
            }
        }

        private ObservableCollectionEx<FieldValue> staffs;
        public ObservableCollectionEx<FieldValue> Staffs
        {
            get
            {
                if (staffs == null)
                    staffs = new ObservableCollectionEx<FieldValue>();
                return staffs;
            }
            set { SetProperty(ref staffs, value); }
        }

        private int selectedStaffId;
        public int SelectedStaffId
        {
            get { return selectedStaffId; }
            set
            {
                if (SetProperty(ref selectedStaffId, value))
                {
                    SelectedPersonStaffId = SpecialValues.NonExistingId;
                    CanSelectPersonStaff = (value == SpecialValues.NonExistingId);
                    OnPropertyChanged(() => SelectedPersonStaffId);
                    IsChanged = true;
                }
            }
        }

        private bool canSelectPersonStaff;
        public bool CanSelectPersonStaff
        {
            get { return canSelectPersonStaff; }
            set { SetProperty(ref canSelectPersonStaff, value); }
        }

        private bool canSelectStaff;
        public bool CanSelectStaff
        {
            get { return canSelectStaff; }
            set { SetProperty(ref canSelectStaff, value); }
        }

        #endregion

        #region IDataError
        private bool saveWasRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        public bool IsValid
        {
            get
            {
                saveWasRequested = true;
                OnPropertyChanged(string.Empty);
                return invalidProperties.Count < 1;
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                if (columnName == "SelectedMemberTypeId")
                    result = SpecialValues.IsNewOrNonExisting(SelectedMemberTypeId) ? "Укажите тип участника" : string.Empty;
                if (columnName == "SelectedPersonStaffId")
                    result = SpecialValues.IsNewOrNonExisting(SelectedPersonStaffId) && SpecialValues.IsNewOrNonExisting(SelectedStaffId) ? "Укажите участника" : string.Empty;
                if (columnName == "SelectedStaffId")
                    result = SpecialValues.IsNewOrNonExisting(SelectedPersonStaffId) && SpecialValues.IsNewOrNonExisting(SelectedStaffId) ? "Укажите должность участника" : string.Empty;

                if (string.IsNullOrEmpty(result))
                    invalidProperties.Remove(columnName);
                else
                    invalidProperties.Add(columnName);
                return result;
            }
        }

        #endregion

        internal async Task Initialize(DateTime onDate)
        {
            MemberTypes.Clear();
            PersonStaffs.Clear();
            Staffs.Clear();
            CanSelectPersonStaff = true;
            CanSelectStaff = true;
            saveWasRequested = false;

            if (cancellationToken != null)
            {
                cancellationToken.Cancel();
                cancellationToken.Dispose();
            }

            cancellationToken = new CancellationTokenSource();
            var token = cancellationToken.Token;

            BusyMediator.Activate("Загрузка данных...");
            logService.Info("Loading data sources for members...");
            try
            {
                var memberTypesList = await Task.Factory.StartNew((Func<IEnumerable<CommissionMemberType>>)commissionService.GetCommissionMemberTypes);
                MemberTypes.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- укажите тип участника -" });
                if (!token.IsCancellationRequested)
                    MemberTypes.AddRange(memberTypesList.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
                SelectedMemberTypeId = SpecialValues.NonExistingId;


                var personStaffsQuery = await commissionService.GetPersonStaffs(onDate).Select(x => new { Id = x.Id, StaffName = x.Staff.Name, PersonName = x.Person.ShortName }).ToArrayAsync(token);
                PersonStaffs.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- укажите участника-" });
                PersonStaffs.AddRange(personStaffsQuery.Select(x => new FieldValue { Value = x.Id, Field = x.StaffName + ": " + x.PersonName }));
                SelectedPersonStaffId = SpecialValues.NonExistingId;

                var staffsList = await Task.Factory.StartNew((Func<IEnumerable<Staff>>)commissionService.GetStaffs);
                Staffs.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- укажите должность участника -" });
                if (!token.IsCancellationRequested)
                    Staffs.AddRange(staffsList.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
                SelectedStaffId = SpecialValues.NonExistingId;

                BeginDateTime = DateTime.Now.Date;
                EndDateTime = SpecialValues.MaxDate;
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources for commissions types");
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        public void CommissionStagesChanged(int[] stages)
        {
            ContextMenuItems.Clear();
            ContextMenuItems.AddRange(stages.Select(x => new CommissionMemberStageViewModel { Stage = x, CommissionMemberId = Id }));
        }
    }
}
