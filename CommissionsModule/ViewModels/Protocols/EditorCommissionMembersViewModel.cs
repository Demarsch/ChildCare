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

namespace CommissionsModule.ViewModels
{
    public class EditorCommissionMembersViewModel : BindableBase, IDialogViewModel, IDataErrorInfo, IDisposable
    {
        private readonly ICommissionService commissionService;
        private readonly ILog logService;
        private readonly Func<CommissionMemberViewModel> commissionMemberViewModelFactory;

        public EditorCommissionMembersViewModel(ICommissionService commissionService,
                                      ILog logService,
                                      Func<CommissionMemberViewModel> commissionMemberViewModelFactory)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }          
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }
            if (commissionMemberViewModelFactory == null)
            {
                throw new ArgumentNullException("commissionMemberViewModelFactory");
            }        
            this.logService = logService;
            this.commissionService = commissionService;
            this.commissionMemberViewModelFactory = commissionMemberViewModelFactory;

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            CloseCommand = new DelegateCommand<bool?>(Close);

            RemoveMemberCommand = new DelegateCommand<CommissionMemberViewModel>(RemoveCommissionMember);
            addMemberCommand = new DelegateCommand(AddMember);

            Members = new ObservableCollectionEx<CommissionMemberViewModel>();
            Members.CollectionChanged += OnMembersChanged;
        }

        #region Properties

        public BusyMediator BusyMediator { get; set; }

        public FailureMediator FailureMediator { get; private set; }

        public ICommand RemoveMemberCommand { get; private set; }

        public ObservableCollectionEx<CommissionMemberViewModel> Members { get; private set; }

        private ObservableCollectionEx<FieldValue> commissionsTypes;
        public ObservableCollectionEx<FieldValue> CommissionsTypes
        {
            get 
            {
                if (commissionsTypes == null)
                    commissionsTypes = new ObservableCollectionEx<FieldValue>();
                return commissionsTypes; 
            }
            set { SetProperty(ref commissionsTypes, value); }
        }

        private int selectedCommissionTypeId;
        public int SelectedCommissionTypeId
        {
            get { return selectedCommissionTypeId; }
            set 
            {
                if (SetProperty(ref selectedCommissionTypeId, value))
                {
                    CanAddMember = !SpecialValues.IsNewOrNonExisting(value);
                    LoadCommissionMembersAsync();
                }
            }
        }

        private DateTime onDate;
        public DateTime OnDate
        {
            get { return onDate; }
            set
            {
                if (SetProperty(ref onDate, value))
                    LoadCommissionMembersAsync();
            }
        }

        private bool canAddMember;
        public bool CanAddMember
        {
            get { return canAddMember; }
            set { SetProperty(ref canAddMember, value); }
        }

        private void OnMembersChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            IsChanged = true;
        }

        private bool isChanged;
        public bool IsChanged
        {
            get { return isChanged; }
            set { SetProperty(ref isChanged, value); }
        }

        private readonly DelegateCommand addMemberCommand;
        public ICommand AddMemberCommand { get { return addMemberCommand; } }

        #endregion

        private void RemoveCommissionMember(CommissionMemberViewModel member)
        {
            FailureMediator.Deactivate();
            if (!SpecialValues.IsNewOrNonExisting(member.Id))
            { 
                var commissionMember = commissionService.CommissionMemberById(member.Id).FirstOrDefault();
                if (commissionMember != null && !commissionMember.CommissionDecisions.Any() && !commissionMember.CommissionDecisions1.Any())
                    Members.Remove(member);
                else
                    FailureMediator.Activate("Удаление невозможно. Данный участник уже выносил решения в комиссиях или являлся инициатором комиссии. Вы можете скорректировать срок действия его полномочий.", true);
            }
            else
                Members.Remove(member);            
        }

        private async void AddMember()
        {
            var memberViewModel = commissionMemberViewModelFactory();
            await memberViewModel.Initialize();
            Members.Add(memberViewModel);
        } 
    
        private async void SaveCommissionMembers()
        {
            FailureMediator.Deactivate();
            try
            {
                BusyMediator.Activate("Сохранение данных...");
                logService.Info("Saving CommissionMembers...");
                await commissionService.SaveCommissionMembersAsync(Members
                                                                .Select(x => new CommissionMember
                                                                {
                                                                    Id = x.Id,
                                                                    PersonStaffId = !SpecialValues.IsNewOrNonExisting(x.SelectedPersonStaffId) ? x.SelectedPersonStaffId : (int?)null,
                                                                    StaffId = !SpecialValues.IsNewOrNonExisting(x.SelectedStaffId) ? x.SelectedStaffId : (int?)null,
                                                                    CommissionMemberTypeId = x.SelectedMemberTypeId,
                                                                    CommissionTypeId = SelectedCommissionTypeId,
                                                                    BeginDateTime = x.BeginDateTime,
                                                                    EndDateTime = x.EndDateTime,
                                                                }).ToArray(), OnDate);
                LoadCommissionMembersAsync();
                logService.Info("CommissionMembers changes successfully saved");
            }
            catch (Exception ex)
            {
                logService.Error("Failed to save CommissionMembers changes", ex);
                FailureMediator.Activate("Не удалось сохранить изменения в составе комиссии. Попробуйте еще раз, если ошибка повторится, обратитесь в службу поддержки", null, ex, true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }  
        }
 
        #region IDialogViewModel

        public string Title
        {
            get { return "Состав комиссии"; }
        }

        public string ConfirmButtonText
        {
            get { return "Сохранить"; }
        }

        public string CancelButtonText
        {
            get { return "Закрыть"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        private void Close(bool? validate)
        {
            if (validate == true)
            {
                if (IsValid && Members.All(x => x.IsValid))
                {
                    SaveCommissionMembers();
                    //OnCloseRequested(new ReturnEventArgs<bool>(true));
                }
                return;
            }
            else
                OnCloseRequested(new ReturnEventArgs<bool>(false));
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

        #region IDataError

        private bool saveWasRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        private bool IsValid
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
                if (columnName == "SelectedCommissionTypeId")
                    result = SpecialValues.IsNewOrNonExisting(SelectedCommissionTypeId) ? "Укажите тип комиссии" : string.Empty;
                if (string.IsNullOrEmpty(result))
                    invalidProperties.Remove(columnName);
                else
                    invalidProperties.Add(columnName);
                return result;
            }
        }

        #endregion

        internal void Initialize()
        {
            LoadCommissionTypes();
        }

        private async void LoadCommissionTypes()
        {
            CommissionsTypes.Clear();
            BusyMediator.Activate("Загрузка данных...");
            logService.Info("Loading commission types...");
            try
            {                
                var commissionTypesTask = Task.Factory.StartNew((Func<object, IEnumerable<CommissionType>>)commissionService.GetCommissionTypes, DateTime.Now);
                await Task.WhenAny(commissionTypesTask);
                var commissionTypesQuery = commissionTypesTask.Result.Select(x => new { x.Id, x.Name, x.ShortName }).ToArray();
                CommissionsTypes.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- укажите тип комиссии -" });
                CommissionsTypes.AddRange(commissionTypesQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
                SelectedCommissionTypeId = SpecialValues.NonExistingId;
                OnDate = DateTime.Now.Date;
                CanAddMember = false;
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load commission types");
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private async void LoadCommissionMembersAsync()
        {
            Members.Clear();
            if (SpecialValues.IsNewOrNonExisting(SelectedCommissionTypeId))
                return;
            BusyMediator.Activate("Загрузка данных...");
            logService.Info("Loading commission members...");
            IDisposableQueryable<CommissionMember> commissionMembersQuery = null;
            try
            {
                commissionMembersQuery = commissionService.GetCommissionMembers(SelectedCommissionTypeId, OnDate);
                var commissionMembersSelectedQuery = await commissionMembersQuery
                                                            .Select(x => new
                                                            {
                                                               Id = x.Id, 
                                                               MemberTypeId = x.CommissionMemberTypeId,
                                                               PersonStaffId = x.PersonStaffId,
                                                               StaffId = x.StaffId,
                                                               BeginDateTime = x.BeginDateTime,
                                                               EndDateTime = x.EndDateTime,
                                                               IsChief = x.CommissionMemberType.IsChief,
                                                               IsSecretary= x.CommissionMemberType.IsSecretary,
                                                               IsMember = x.CommissionMemberType.IsMember,
                                                            }).ToArrayAsync();
                foreach (var member in commissionMembersSelectedQuery.OrderByDescending(x => x.IsChief).ThenByDescending(x => x.IsSecretary).ThenByDescending(x => x.IsMember))
                {
                    var memberViewModel = new CommissionMemberViewModel(commissionService, logService);
                    await memberViewModel.Initialize();
                    memberViewModel.Id = member.Id;
                    memberViewModel.SelectedMemberTypeId = member.MemberTypeId;
                    memberViewModel.BeginDateTime = member.BeginDateTime.Date;
                    memberViewModel.EndDateTime = member.EndDateTime.Date;
                    if (member.PersonStaffId.HasValue)
                        memberViewModel.SelectedPersonStaffId = member.PersonStaffId.Value;
                    else if (member.StaffId.HasValue)
                        memberViewModel.SelectedStaffId = member.StaffId.Value;
                    Members.Add(memberViewModel);
                }
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load commission members");
            }
            finally
            {
                BusyMediator.Deactivate();
                if (commissionMembersQuery != null)
                    commissionMembersQuery.Dispose();
            }
        }

        public void Dispose()
        {
            Members.CollectionChanged -= OnMembersChanged;
        }       
    }
}
