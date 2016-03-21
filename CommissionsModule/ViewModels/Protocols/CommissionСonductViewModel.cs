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

namespace CommissionsModule.ViewModels
{
    public class CommissionСonductViewModel : BindableBase, IDisposable
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly ISecurityService securityService;
        private readonly ILog logService;

        private readonly Func<CommissionDecisionViewModel> commissionDecisionViewModelFactory;
        private readonly Func<CommissionMemberViewModel> commissionMemberViewModelFactory;

        private readonly CommandWrapper reInitializeCommandWrapper;

        private CancellationTokenSource currentOperationToken;

        #endregion

        #region Constructors
        public CommissionСonductViewModel(ICommissionService commissionService, ISecurityService securityService, ILog logService,
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
            this.securityService = securityService;
            this.commissionDecisionViewModelFactory = commissionDecisionViewModelFactory;
            this.commissionMemberViewModelFactory = commissionMemberViewModelFactory;
            this.commissionService = commissionService;
            this.logService = logService;
            AvailableMembers = new ObservableCollectionEx<CommissionMemberViewModel>();
            CurrentMembers = new ObservableCollectionEx<CommissionDecisionViewModel>();

            addSelectedAvailableMemberCommand = new DelegateCommand<CommissionMemberStageViewModel>(AddSelectedAvailableMember);
            removeCurrentMemberCommand = new DelegateCommand<CommissionDecisionViewModel>(RemoveCurrentMember, CanRemoveCurrentMember);
            changeNeedAllMembersCommand = new DelegateCommand<CommissionMemberGroupItem>(ChangeNeedAllMembers);
            removeStageCommand = new DelegateCommand<CommissionMemberGroupItem>(RemoveStage, CanRemoveStage);
            upStageCommand = new DelegateCommand<CommissionMemberGroupItem>(UpStage);
            downStageCommand = new DelegateCommand<CommissionMemberGroupItem>(DownStage);

            reInitializeCommandWrapper = new CommandWrapper() { Command = new DelegateCommand(() => Initialize(CommissionProtocolId)), CommandName = "Повторить" };

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
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


        public ObservableCollectionEx<CommissionDecisionViewModel> CurrentMembers { get; set; }
        public ObservableCollectionEx<CommissionMemberViewModel> AvailableMembers { get; set; }

        public BusyMediator BusyMediator { get; private set; }
        public FailureMediator FailureMediator { get; private set; }

        public int CommissionProtocolId { get; private set; }
        public int PersonId { get; private set; }
        #endregion

        #region Methods
        public async void Initialize(int commissionProtocolId = SpecialValues.NonExistingId, int personId = SpecialValues.NonExistingId)
        {
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }

            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            CurrentMembers.CollectionChanged -= CurrentMembers_CollectionChanged;
            AvailableMembers.Clear();
            CurrentMembers.Clear();
            CommissionProtocolId = commissionProtocolId;
            PersonId = personId;

            var loadingIsCompleted = false;
            BusyMediator.Activate("Загрузка хода комиссии...");
            logService.InfoFormat("Loading commission conduction with protocol id ={0}", commissionProtocolId);
            var commissionProtocolQuery = commissionService.GetCommissionProtocolById(commissionProtocolId);
            var commissionDecisionsQuery = commissionService.GetCommissionDecisions(commissionProtocolId);
            IDisposableQueryable<CommissionMember> commissionMembersQuery = null;
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

                    commissionMembersQuery = commissionService.GetCommissionMembers(commissionProtocolData.CommissionTypeId, curDate);
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
                }
                var commissionDecisionIds = await commissionDecisionsQuery.Select(x => x.Id).ToArrayAsync(token);
                List<Task> decisionsTask = new List<Task>();
                foreach (var commissionDecisionId in commissionDecisionIds)
                {
                    var commissionDecisionViewModel = commissionDecisionViewModelFactory();
                    //await commissionDecisionViewModel.Initialize(commissionDecisionId);
                    decisionsTask.Add(commissionDecisionViewModel.Initialize(commissionDecisionId, true));
                    CurrentMembers.Add(commissionDecisionViewModel);
                }
                await Task.WhenAll(decisionsTask);
                SetStagesMenuItems();
                SetCurrentMembersIsNotLastItem();
                CurrentMembers.CollectionChanged += CurrentMembers_CollectionChanged;
                //removeStageCommand.RaiseCanExecuteChanged();
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load commission conduction with protocol id ={0}", commissionProtocolId);
                FailureMediator.Activate("Не удалость загрузить ход комиссии. Попробуйте еще раз или обратитесь в службу поддержки", reInitializeCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                if (loadingIsCompleted)
                    BusyMediator.Deactivate();
                if (commissionDecisionsQuery != null)
                    commissionDecisionsQuery.Dispose();
                if (commissionProtocolQuery != null)
                    commissionProtocolQuery.Dispose();
                if (commissionMembersQuery != null)
                    commissionMembersQuery.Dispose();
            }
        }

        void CurrentMembers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetStagesMenuItems();
            SetCurrentMembersIsNotLastItem();
        }

        private void SetCurrentMembersIsNotLastItem()
        {
            var maxStage = CurrentMembers.Max(x => x.Stage);
            foreach (var member in CurrentMembers)
            {
                member.IsNotLastItem = (member.Stage != maxStage);
            }
        }

        private void SetStagesMenuItems()
        {
            var stages = CurrentMembers.Select(x => x.Stage).Distinct().OrderBy(x => x).ToArray();
            foreach (var availableMember in AvailableMembers)
            {
                availableMember.CommissionStagesChanged(stages);
            }
        }

        public void Dispose()
        {
            CurrentMembers.CollectionChanged -= CurrentMembers_CollectionChanged;
        }

        public void GetСonductionCommissionProtocolData(ref CommissionProtocol commissionProtocol)
        {
            throw new NotImplementedException();
        }

        private async void AddSelectedAvailableMember(CommissionMemberStageViewModel selectedCommissionMemberStageViewModel)
        {
            var commissionDecisionViewModel = commissionDecisionViewModelFactory();
            await commissionDecisionViewModel.InitializeNew(selectedCommissionMemberStageViewModel.CommissionMemberId, selectedCommissionMemberStageViewModel.Stage, CommissionProtocolId, true);
            CurrentMembers.Add(commissionDecisionViewModel);
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
            return !CurrentMembers.Any(x => x.Stage == сommissionMemberGroupItem.Stage && !x.CanDeleteMember);
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
