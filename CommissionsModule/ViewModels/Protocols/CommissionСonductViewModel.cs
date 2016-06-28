using CommissionsModule.Services;
using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Mvvm;
using log4net;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity;
using Core.Extensions;
using Core.Wpf.Misc;
using Prism.Commands;
using Core.Misc;
using System.Windows.Input;
using Core.Services;
using CommissionsModule.ViewModels.Protocols;
using Core.Data.Services;

namespace CommissionsModule.ViewModels
{
    public class CommissionСonductViewModel : TrackableBindableBase, IChangeTrackerMediator, IDisposable
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly ISecurityService securityService;
        private readonly IUserService userService;
        private readonly ILog logService;

        private readonly Func<CommissionDecisionViewModel> commissionDecisionViewModelFactory;
        private readonly Func<CommissionMemberViewModel> commissionMemberViewModelFactory;

        private readonly CommandWrapper reInitializeCommandWrapper;

        private CancellationTokenSource currentOperationToken;
        private CancellationTokenSource loadAvailiableMembersOperationToken;

        private ObservableCollectionChangeTracker<CommissionDecisionViewModel> currentMembersChangeTracker;

        #endregion

        #region Constructors
        public CommissionСonductViewModel(ICommissionService commissionService, ISecurityService securityService, IUserService userService, ILog logService,
            Func<CommissionDecisionViewModel> commissionDecisionViewModelFactory, Func<CommissionMemberViewModel> commissionMemberViewModelFactory)
        {
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (commissionDecisionViewModelFactory == null)
            {
                throw new ArgumentNullException("commissionDecisionViewModelFactory");
            }
            if (commissionMemberViewModelFactory == null)
            {
                throw new ArgumentNullException("commissionMemberViewModelFactory");
            }
            if (securityService == null)
            {
                throw new ArgumentNullException("securityService");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            this.userService = userService;
            this.securityService = securityService;
            this.commissionDecisionViewModelFactory = commissionDecisionViewModelFactory;
            this.commissionMemberViewModelFactory = commissionMemberViewModelFactory;
            this.commissionService = commissionService;
            this.logService = logService;
            AvailableMembers = new ObservableCollectionEx<CommissionMemberViewModel>();
            CurrentMembers = new ObservableCollectionEx<CommissionDecisionViewModel>();
            CurrentMembers.ItemPropertyChanged += CurrentMembers_ItemPropertyChanged;

            addSelectedAvailableMemberCommand = new DelegateCommand<CommissionMemberStageViewModel>(AddSelectedAvailableMember);
            removeCurrentMemberCommand = new DelegateCommand<CommissionDecisionViewModel>(RemoveCurrentMember, CanRemoveCurrentMember);
            changeNeedAllMembersCommand = new DelegateCommand<CommissionMemberGroupItem>(ChangeNeedAllMembers);
            removeStageCommand = new DelegateCommand<CommissionMemberGroupItem>(RemoveStage, CanRemoveStage);
            upStageCommand = new DelegateCommand<CommissionMemberGroupItem>(UpStage);
            downStageCommand = new DelegateCommand<CommissionMemberGroupItem>(DownStage);

            reInitializeCommandWrapper = new CommandWrapper() { Command = new DelegateCommand(() => Initialize(CommissionProtocolId, PersonId)), CommandName = "Повторить" };

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            currentMembersChangeTracker = new ObservableCollectionChangeTracker<CommissionDecisionViewModel>(CurrentMembers);
            ChangeTracker = new CompositeChangeTracker(currentMembersChangeTracker);
            
        }

        #endregion

        #region Properties

        private CommissionMemberViewModel selectedAvailableMember;
        public CommissionMemberViewModel SelectedAvailableMember
        {
            get { return selectedAvailableMember; }
            set { SetProperty(ref selectedAvailableMember, value); }
        }

        private CommissionDecisionViewModel selectedCurrentMember;
        public CommissionDecisionViewModel SelectedCurrentMember
        {
            get { return selectedCurrentMember; }
            set { SetProperty(ref selectedCurrentMember, value); }
        }

        private ObservableCollectionEx<CommissionDecisionViewModel> currentMembers;
        public ObservableCollectionEx<CommissionDecisionViewModel> CurrentMembers
        {
            get { return currentMembers; }
            set { SetTrackedProperty(ref currentMembers, value); }
        }
        public ObservableCollectionEx<CommissionMemberViewModel> AvailableMembers { get; set; }

        public BusyMediator BusyMediator { get; private set; }
        public FailureMediator FailureMediator { get; private set; }
        public IChangeTracker ChangeTracker { get; private set; }

        public int CommissionProtocolId { get; private set; }
        public int PersonId { get; private set; }
        #endregion

        #region Methods
        public async void Initialize(int commissionProtocolId = SpecialValues.NonExistingId, int personId = SpecialValues.NonExistingId)
        {
            ChangeTracker.IsEnabled = false;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }

            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;

            CommissionProtocolId = commissionProtocolId;
            PersonId = personId;

            BusyMediator.Activate("Загрузка хода комиссии...");
            logService.InfoFormat("Loading commission conduction with protocol id ={0}", commissionProtocolId);
            var commissionProtocolQuery = commissionService.GetCommissionProtocolById(commissionProtocolId);
            var curDate = DateTime.Now;
            try
            {
                var commissionProtocolData = await commissionProtocolQuery.Select(x => new
                {
                    x.IncomeDateTime,
                    x.PersonId,
                    x.CommissionTypeId
                }).FirstOrDefaultAsync(token);
                if (commissionProtocolData != null)
                {
                    PersonId = commissionProtocolData.PersonId;
                    curDate = commissionProtocolData.IncomeDateTime;
                    LoadAvailableMembers(commissionProtocolData.CommissionTypeId, curDate);
                }
                LoadCurrentMembers(commissionProtocolId, token);
                ChangeTracker.IsEnabled = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load commission conduction with protocol id ={0}", commissionProtocolId);
                FailureMediator.Activate("Не удалость загрузить ход комиссии. Попробуйте еще раз или обратитесь в службу поддержки", reInitializeCommandWrapper, ex);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (commissionProtocolQuery != null)
                    commissionProtocolQuery.Dispose();
            }
        }

        public async void LoadCurrentMembers(int commissionProtocolId, CancellationToken token)
        {
            
            CurrentMembers.CollectionChanged -= CurrentMembers_CollectionChanged;
            CurrentMembers.Clear();
            var commissionDecisionsQuery = commissionService.GetCommissionDecisions(commissionProtocolId);
            try
            {
                DateTime maxDate = SpecialValues.MaxDate;
                var commissionDecisionIds = await commissionDecisionsQuery.Where(x => x.RemovedByUserId == null).OrderBy(x => x.CommissionStage).ThenBy(x => x.DecisionDateTime == null ? maxDate : x.DecisionDateTime).Select(x => x.Id).ToArrayAsync(token);
                List<Task> decisionsTask = new List<Task>();
                List<CommissionDecisionViewModel> list = new List<CommissionDecisionViewModel>();
                foreach (var commissionDecisionId in commissionDecisionIds)
                {
                    var commissionDecisionViewModel = commissionDecisionViewModelFactory();
                    commissionDecisionViewModel.PropertyChanged += commissionDecisionViewModel_PropertyChanged;
                    //(ChangeTracker as CompositeChangeTracker).AddTracker(commissionDecisionViewModel.ChangeTracker);
                    //await commissionDecisionViewModel.Initialize(commissionDecisionId);
                    decisionsTask.Add(commissionDecisionViewModel.Initialize(commissionDecisionId));
                    list.Add(commissionDecisionViewModel);
                }
                await Task.WhenAll(decisionsTask);
                if (!token.IsCancellationRequested)
                {
                    CurrentMembers.AddRange(list);
                    SetCurrentMembersIsNotLastItem();
                    SetStageState();
                    SetStagesMenuItems();
                    removeStageCommand.RaiseCanExecuteChanged();
                    CurrentMembers.CollectionChanged += CurrentMembers_CollectionChanged;
                }
                ChangeTracker.IsEnabled = false;
                ChangeTracker.IsEnabled = true;
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load Current Members of commission conduction with protocol id ={0}", commissionProtocolId);
                FailureMediator.Activate("Не удалость ход комиссии. Попробуйте еще раз или обратитесь в службу поддержки", reInitializeCommandWrapper, ex);
            }
            finally
            {
                if (commissionDecisionsQuery != null)
                    commissionDecisionsQuery.Dispose();
            }
        }

        public async void LoadAvailableMembers(int commissionTypeId, DateTime onDate)
        {
            AvailableMembers.Clear();
            if (loadAvailiableMembersOperationToken != null)
            {
                loadAvailiableMembersOperationToken.Cancel();
                loadAvailiableMembersOperationToken.Dispose();
            }

            loadAvailiableMembersOperationToken = new CancellationTokenSource();
            var token = loadAvailiableMembersOperationToken.Token;

            var commissionMembersQuery = commissionService.GetCommissionMembers(commissionTypeId, onDate);
            try
            {
                var commissionMembers = await commissionMembersQuery.Select(x => new
                {
                    Id = x.Id,
                    MemberTypeName = x.CommissionMemberType.Name,
                    PersonName = x.PersonStaffId.HasValue ? x.PersonStaff.Person.ShortName : string.Empty,
                    StaffName = x.PersonStaffId.HasValue ? x.PersonStaff.Staff.Name : x.StaffId.HasValue ? x.Staff.Name : string.Empty
                }).ToArrayAsync(token);

                var commissionMembersToAdd = new List<CommissionMemberViewModel>();
                foreach (var commissionMember in commissionMembers)
                {
                    if (token.IsCancellationRequested) break;
                    var commissionMemberViewModel = commissionMemberViewModelFactory();
                    commissionMemberViewModel.Id = commissionMember.Id;
                    commissionMemberViewModel.MemberTypeName = commissionMember.MemberTypeName;
                    commissionMemberViewModel.PersonName = commissionMember.PersonName;
                    commissionMemberViewModel.StaffName = commissionMember.StaffName;
                    commissionMembersToAdd.Add(commissionMemberViewModel);
                }
                if (!token.IsCancellationRequested)
                    AvailableMembers.AddRange(commissionMembersToAdd);
                SetStagesMenuItems();
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load availiable members of commission with commissionTypeId={0}", commissionTypeId);
                FailureMediator.Activate("Не удалость загрузить участников комиссии. Попробуйте еще раз или обратитесь в службу поддержки", reInitializeCommandWrapper, ex);
            }
            finally
            {
                if (commissionMembersQuery != null)
                    commissionMembersQuery.Dispose();
            }
        }

        void commissionDecisionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == "Stage" || e.PropertyName == "CommissionMemberGroupItem")
            //{
            //    OnPropertyChanged(() => CurrentMembers);
            //}
            if (e.PropertyName == "CommissionMemberGroupItem")
            {
                removeStageCommand.RaiseCanExecuteChanged();
            }
            removeCurrentMemberCommand.RaiseCanExecuteChanged();
        }

        void CurrentMembers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CurrentMembersChanged();
        }

        void CurrentMembers_ItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == "Stage" || e.PropertyName == "NeedAllMembers")
            CurrentMembersChanged();
        }

        private void CurrentMembersChanged()
        {
            SetStagesMenuItems();
            SetStageState();
            SetCurrentMembersIsNotLastItem();
            removeStageCommand.RaiseCanExecuteChanged();
        }


        private void SetCurrentMembersIsNotLastItem()
        {
            if (CurrentMembers.Count < 1) return;
            var maxStage = CurrentMembers.Max(x => x.Stage);
            foreach (var member in CurrentMembers)
            {
                member.IsNotLastItem = (member.Stage != maxStage);
            }
        }

        private void SetStageState()
        {
            if (CurrentMembers.Count < 1) return;
            foreach (var member in CurrentMembers)
            {
                member.IsHaveAllDecisions = CurrentMembers.Where(x => x.Stage == member.Stage).All(x => x.Decision != string.Empty);
                member.IsHaveAnyDecisions = CurrentMembers.Where(x => x.Stage == member.Stage).Any(x => x.Decision != string.Empty);
            }
        }


        private void SetStagesMenuItems()
        {
            var stages = CurrentMembers.Select(x => x.Stage).Distinct().OrderBy(x => x).ToArray();
            int[] existedInStages;
            foreach (var availableMember in AvailableMembers)
            {
                existedInStages = CurrentMembers.Where(x => x.CommissionMemberId == availableMember.Id).Select(x => x.Stage).ToArray();
                int maxStage = 0;
                if (stages.Any())
                    maxStage = stages.Max();
                var nextStage = ++maxStage;
                availableMember.CommissionStagesChanged(stages.Except<int>(existedInStages).Union(new int[] { nextStage }).ToArray());
            }
        }

        public void Dispose()
        {
            CurrentMembers.CollectionChanged -= CurrentMembers_CollectionChanged;
        }

        public void GetСonductionCommissionProtocolData(ref CommissionProtocol commissionProtocol)
        {
            CommissionDecision decision;
            commissionProtocol.CommissionDecisions.Clear();
            foreach (var item in CurrentMembers)
            {
                decision = new CommissionDecision
                {
                    Id = item.CommissionDecisionId,
                    CommissionProtocol = commissionProtocol,
                    CommissionStage = item.Stage,
                    NeedAllMembersInStage = item.NeedAllMembers,
                    CommissionMemberId = item.CommissionMemberId,
                    InitiatorUserId = userService.GetCurrentUserId()
                };
                commissionProtocol.CommissionDecisions.Add(decision);
            }
        }

        private async void AddSelectedAvailableMember(CommissionMemberStageViewModel selectedCommissionMemberStageViewModel)
        {
            if (!CurrentMembers.Any(x => x.CommissionMemberId == selectedCommissionMemberStageViewModel.CommissionMemberId && x.Stage == selectedCommissionMemberStageViewModel.Stage))
            {
                var commissionDecisionViewModel = commissionDecisionViewModelFactory();
                await commissionDecisionViewModel.InitializeNew(selectedCommissionMemberStageViewModel.CommissionMemberId, selectedCommissionMemberStageViewModel.Stage, CommissionProtocolId);
                CurrentMembers.Add(commissionDecisionViewModel);
                SetStagesMenuItems();
            }
        }

        private bool CanRemoveCurrentMember(CommissionDecisionViewModel commissionDecisionViewModel)
        {
            return commissionDecisionViewModel != null && commissionDecisionViewModel.CanDeleteMember;
        }

        private void RemoveCurrentMember(CommissionDecisionViewModel commissionDecisionViewModel)
        {
            var curStage = commissionDecisionViewModel.Stage;
            CurrentMembers.Remove(commissionDecisionViewModel);
            if (!CurrentMembers.Any(x => x.Stage == curStage))
                ChangeStageInt(curStage);
            //SetStagesMenuItems();
        }

        private void ChangeStageInt(int curStage)
        {
            foreach (var curMembers in CurrentMembers.Where(x => x.Stage > curStage))
            {
                curMembers.Stage--;
            }
        }

        private void ChangeNeedAllMembers(CommissionMemberGroupItem commissionMemberGroupItem)
        {
            foreach (var currentMember in CurrentMembers.Where(x => x.Stage == commissionMemberGroupItem.Stage))
                currentMember.NeedAllMembers = commissionMemberGroupItem.NeedAllMembers;
        }

        private bool CanRemoveStage(CommissionMemberGroupItem сommissionMemberGroupItem)
        {
            if (сommissionMemberGroupItem == null || CurrentMembers == null)
                return false;
            var canRemoveStage = CurrentMembers.Where(x => x.Stage == сommissionMemberGroupItem.Stage).All(x => x.CanDeleteMember) || securityService.HasPermission(Permission.DeleteCommissionDecisionWithDecision);
            return canRemoveStage;
        }

        private void RemoveStage(CommissionMemberGroupItem commissionMemberGroupItem)
        {
            var curStage = commissionMemberGroupItem.Stage;
            CurrentMembers.RemoveWhere(x => x.Stage == commissionMemberGroupItem.Stage);
            ChangeStageInt(curStage);
            //SetStagesMenuItems();
        }

        private void UpStage(CommissionMemberGroupItem commissionMemberGroupItem)
        {
            SwapStage(commissionMemberGroupItem.Stage, commissionMemberGroupItem.Stage - 1);
        }

        private void DownStage(CommissionMemberGroupItem commissionMemberGroupItem)
        {
            SwapStage(commissionMemberGroupItem.Stage, commissionMemberGroupItem.Stage + 1);
        }

        private void SwapStage(int curStage, int newStage)
        {
            var curStageMembers = CurrentMembers.Where(x => x.Stage == curStage).ToArray();
            var newStageMembers = CurrentMembers.Where(x => x.Stage == newStage).ToArray();
            foreach (var member in curStageMembers)
            {
                member.Stage = newStage;
            }
            foreach (var member in newStageMembers)
            {
                member.Stage = curStage;
            }
            SetCurrentMembersIsNotLastItem();
            var list = CurrentMembers.OrderBy(x => x.Stage).ToArray();
            CurrentMembers.Clear();
            CurrentMembers.AddRange(list);
        }
        #endregion

        #region Commands

        private DelegateCommand<CommissionMemberStageViewModel> addSelectedAvailableMemberCommand;
        public ICommand AddSelectedAvailableMemberCommand { get { return addSelectedAvailableMemberCommand; } }

        private DelegateCommand<CommissionDecisionViewModel> removeCurrentMemberCommand;
        public ICommand RemoveCurrentMemberCommand { get { return removeCurrentMemberCommand; } }

        private DelegateCommand<CommissionMemberGroupItem> changeNeedAllMembersCommand;
        public ICommand ChangeNeedAllMembersCommand { get { return changeNeedAllMembersCommand; } }

        private DelegateCommand<CommissionMemberGroupItem> removeStageCommand;
        public ICommand RemoveStageCommand { get { return removeStageCommand; } }

        private DelegateCommand<CommissionMemberGroupItem> upStageCommand;
        public ICommand UpStageCommand { get { return upStageCommand; } }

        private DelegateCommand<CommissionMemberGroupItem> downStageCommand;
        public ICommand DownStageCommand { get { return downStageCommand; } }
        #endregion
    }
}
