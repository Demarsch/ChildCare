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

namespace CommissionsModule.ViewModels
{
    public class CommissionСonductViewModel : BindableBase
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly ILog logService;

        private readonly Func<CommissionDecisionViewModel> commissionDecisionViewModelFactory;

        private readonly CommandWrapper reInitializeCommandWrapper;

        private CancellationTokenSource currentOperationToken;

        #endregion

        #region Constructors
        public CommissionСonductViewModel(ICommissionService commissionService, ILog logService, Func<CommissionDecisionViewModel> commissionDecisionViewModelFactory)
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
            this.commissionDecisionViewModelFactory = commissionDecisionViewModelFactory;
            this.commissionService = commissionService;
            this.logService = logService;
            AvailableMembers = new ObservableCollectionEx<CommissionMemberViewModel>();
            CurrentMembers = new ObservableCollectionEx<CommissionDecisionViewModel>();
            addSelectedAvailableMemberCommand = new DelegateCommand<CommissionMemberViewModel>(AddSelectedAvailableMember);

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

        public ObservableCollectionEx<CommissionMemberViewModel> AvailableMembers { get; set; }
        public ObservableCollectionEx<CommissionDecisionViewModel> CurrentMembers { get; set; }

        public BusyMediator BusyMediator { get; private set; }
        public FailureMediator FailureMediator { get; private set; }

        public int CommissionProtocolId { get; private set; }
        #endregion

        #region Methods
        public async void Initialize(int commissionProtocolId = SpecialValues.NonExistingId, int personId = SpecialValues.NonExistingId)
        {
            AvailableMembers.Clear();
            CurrentMembers.Clear();
            CommissionProtocolId = commissionProtocolId;
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
            var persId = personId;
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
                    persId = commissionProtocolData.PersonId;
                    curDate = commissionProtocolData.IncomeDateTime;

                    commissionMembersQuery = commissionService.GetCommissionMembers(commissionProtocolData.CommissionTypeId, curDate);
                    var commissionMembers = await commissionMembersQuery.Select(x => new CommissionMemberViewModel
                    {
                        Id = x.Id,
                        MemberTypeName = x.CommissionMemberType.Name,
                        PersonName = x.PersonStaffId.HasValue ? x.PersonStaff.Person.ShortName : string.Empty,
                        StaffName = x.PersonStaffId.HasValue ? x.PersonStaff.Staff.Name : x.StaffId.HasValue ? x.Staff.Name : string.Empty
                    }).ToArrayAsync();
                    AvailableMembers.AddRange(commissionMembers);
                }
                var commissionDecisionIds = await commissionDecisionsQuery.Select(x => x.Id).ToArrayAsync(token);
                List<Task> decisionsTask = new List<Task>();
                foreach (var commissionDecisionId in commissionDecisionIds)
                {
                    var commissionDecisionViewModel = commissionDecisionViewModelFactory();
                    //await commissionDecisionViewModel.Initialize(commissionDecisionId);
                    decisionsTask.Add(commissionDecisionViewModel.Initialize(commissionDecisionId));
                    CurrentMembers.Add(commissionDecisionViewModel);
                }
                await Task.WhenAll(decisionsTask);
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

        public void GetСonductionCommissionProtocolData(ref CommissionProtocol commissionProtocol)
        {
            throw new NotImplementedException();
        }

        private async void AddSelectedAvailableMember(CommissionMemberViewModel selectedCommissionMemberViewModel)
        {
            var commissionDecisionViewModel = commissionDecisionViewModelFactory();
            await commissionDecisionViewModel.Initialize(SpecialValues.NewId);
            CurrentMembers.Add(commissionDecisionViewModel);
        }
        #endregion

        #region Commands

        private DelegateCommand<CommissionMemberViewModel> addSelectedAvailableMemberCommand;
        public ICommand AddSelectedAvailableMemberCommand { get { return addSelectedAvailableMemberCommand; } }
        #endregion
    }
}
