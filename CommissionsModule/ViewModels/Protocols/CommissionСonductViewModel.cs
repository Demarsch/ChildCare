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
            removeCurrentMemberCommand = new DelegateCommand<CommissionDecisionViewModel>(RemoveCurrentMember);
            changeNeedAllMembersCommand = new DelegateCommand<CommissionMemberGroupItem>(ChangeNeedAllMembers);
            removeStageCommand = new DelegateCommand<CommissionMemberGroupItem>(RemoveStage);

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
            CurrentMembers.CollectionChanged -= CurrentMembers_CollectionChanged;
            AvailableMembers.Clear();
            CurrentMembers.Clear();
            CommissionProtocolId = commissionProtocolId;
            PersonId = personId;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
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
                    }).ToArrayAsync();
                    foreach (var commissionMember in commissionMembers)
                    {
                        var commissionMemberViewModel = commissionMemberViewModelFactory();
                        commissionMemberViewModel.Id = commissionMember.Id;
                        commissionMemberViewModel.MemberTypeName = commissionMember.MemberTypeName;
                        commissionMemberViewModel.PersonName = commissionMember.PersonName;
                        commissionMemberViewModel.StaffName = commissionMember.StaffName;
                        AvailableMembers.Add(commissionMemberViewModel);
                    }
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
                CurrentMembers.CollectionChanged += CurrentMembers_CollectionChanged;
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
            await commissionDecisionViewModel.InitializeNew(selectedCommissionMemberStageViewModel.CommissionMemberId, selectedCommissionMemberStageViewModel.Stage, true);
            CurrentMembers.Add(commissionDecisionViewModel);
        }

        private void RemoveCurrentMember(CommissionDecisionViewModel commissionDecisionViewModel)
        {
            var curStage = commissionDecisionViewModel.Stage;
            CurrentMembers.Remove(commissionDecisionViewModel);
            if (!CurrentMembers.Any(x => x.Stage == curStage))
                ChangeStageInt(curStage);
            SetStagesMenuItems();
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

        private void RemoveStage(CommissionMemberGroupItem commissionMemberGroupItem)
        {
            var curStage = commissionMemberGroupItem.Stage;
            CurrentMembers.RemoveWhere(x => x.Stage == commissionMemberGroupItem.Stage);
            ChangeStageInt(curStage);
            SetStagesMenuItems();
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
        #endregion
    }
}
