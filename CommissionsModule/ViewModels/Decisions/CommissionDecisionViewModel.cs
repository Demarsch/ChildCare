﻿using CommissionsModule.Services;
using log4net;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity;
using Core.Wpf.Mvvm;
using Core.Data.Misc;
using Core.Data;
using Core.Extensions;
using Core.Wpf.Misc;
using Prism.Commands;
using Core.Services;
using Core.Misc;

namespace CommissionsModule.ViewModels
{
    public class CommissionDecisionViewModel : TrackableBindableBase, IDisposable, IChangeTrackerMediator
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly ISecurityService securityService;
        private readonly ILog logService;

        private CommandWrapper reloadInitialzeCommandWrapper;

        private CancellationTokenSource currentOperationToken;

        private int commissionProtocolId = SpecialValues.NonExistingId;

        private string emptyDecisionColor = "#E5E5E5";
        #endregion

        #region Constructors
        public CommissionDecisionViewModel(ICommissionService commissionService, ISecurityService securityService, ILog logService)
        {
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (securityService == null)
            {
                throw new ArgumentNullException("securityService");
            }
            this.securityService = securityService;
            this.commissionService = commissionService;
            this.logService = logService;
            ChangeTracker = new ChangeTrackerEx<CommissionDecisionViewModel>(this);
            BusyMediator = new BusyMediator();
        }

        #endregion

        #region Properties
        private int commissionDecisionId = SpecialValues.NonExistingId;
        public int CommissionDecisionId
        {
            get { return commissionDecisionId; }
            set { SetProperty(ref commissionDecisionId, value); }
        }

        private int commissionMemberId = SpecialValues.NonExistingId;
        public int CommissionMemberId
        {
            get { return commissionMemberId; }
            set { SetProperty(ref commissionMemberId, value); }
        }

        private int stage = 0;
        public int Stage
        {
            get { return stage; }
            set
            {
                SetTrackedProperty(ref stage, value);
                if (CommissionMemberGroupItem != null)
                    CommissionMemberGroupItem.Stage = stage;
            }
        }

        private bool isNotLastItem;
        public bool IsNotLastItem
        {
            get { return isNotLastItem; }
            set
            {
                SetProperty(ref isNotLastItem, value);
                if (CommissionMemberGroupItem != null)
                    CommissionMemberGroupItem.IsNotLastItem = isNotLastItem;
            }
        }

        private bool needAllMembers;
        public bool NeedAllMembers
        {
            get { return needAllMembers; }
            set
            {
                SetTrackedProperty(ref needAllMembers, value);
                if (CommissionMemberGroupItem != null)
                    CommissionMemberGroupItem.NeedAllMembers = needAllMembers;
            }
        }

        private bool isOfficial;
        public bool IsOfficial
        {
            get { return isOfficial; }
            set { SetTrackedProperty(ref isOfficial, value); }
        }

        private bool сanChangeIsOfficial;
        public bool CanChangeIsOfficial
        {
            get { return сanChangeIsOfficial; }
            set { SetProperty(ref сanChangeIsOfficial, value); }
        }

        private bool isHaveAllDecisions;
        public bool IsHaveAllDecisions
        {
            get { return isHaveAllDecisions; }
            set
            {
                SetProperty(ref isHaveAllDecisions, value);
                if (CommissionMemberGroupItem != null)
                    CommissionMemberGroupItem.IsHaveAllDecisions = isHaveAllDecisions;
            }
        }

        private bool isHaveAnyDecisions;
        public bool IsHaveAnyDecisions
        {
            get { return isHaveAnyDecisions; }
            set
            {
                SetProperty(ref isHaveAnyDecisions, value);
                if (CommissionMemberGroupItem != null)
                    CommissionMemberGroupItem.IsHaveAnyDecisions = isHaveAnyDecisions;
            }
        }

        private CommissionMemberGroupItem commissionMemberGroupItem = new CommissionMemberGroupItem();
        public CommissionMemberGroupItem CommissionMemberGroupItem
        {
            get { return commissionMemberGroupItem; }
            set { SetProperty(ref commissionMemberGroupItem, value); }
        }

        private DateTime? decisionDate;
        public DateTime? DecisionDate
        {
            get { return decisionDate; }
            set { SetProperty(ref decisionDate, value); }
        }

        private string memberName;
        public string MemberName
        {
            get { return memberName; }
            set { SetProperty(ref memberName, value); }
        }

        private string decision;
        public string Decision
        {
            get { return decision; }
            set { SetProperty(ref decision, value); }
        }

        private string comment;
        public string Comment
        {
            get { return comment; }
            set { SetProperty(ref comment, value); }
        }

        private string colorType;
        public string ColorType
        {
            get { return colorType; }
            set { SetProperty(ref colorType, value); }
        }

        private bool canDeleteMember;
        public bool CanDeleteMember
        {
            get { return canDeleteMember; }
            private set { SetProperty(ref canDeleteMember, value); }
        }

        public IChangeTracker ChangeTracker { get; private set; }

        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; set; }
        #endregion

        #region Methods

        public void EraseProperties()
        {
            CommissionMemberGroupItem.PropertyChanged -= CommissionMemberGroupItem_PropertyChanged;
            CommissionDecisionId = SpecialValues.NonExistingId;
            CommissionMemberId = SpecialValues.NonExistingId;
            commissionProtocolId = SpecialValues.NonExistingId;
            DecisionDate = null;
            Stage = 0;
            IsOfficial = false;
            CanChangeIsOfficial = false;
            NeedAllMembers = false;
            IsHaveAllDecisions = false;
            IsHaveAnyDecisions = false;
            MemberName = string.Empty;
            Decision = string.Empty;
            ColorType = emptyDecisionColor;
            Comment = string.Empty;
            CommissionMemberGroupItem = new CommissionMemberGroupItem();
            CommissionMemberGroupItem.PropertyChanged += CommissionMemberGroupItem_PropertyChanged;
        }

        public void Dispose()
        {
            if (CommissionMemberGroupItem != null)
                CommissionMemberGroupItem.PropertyChanged -= CommissionMemberGroupItem_PropertyChanged;
        }

        void CommissionMemberGroupItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NeedAllMembers" && NeedAllMembers != CommissionMemberGroupItem.NeedAllMembers)
            {
                NeedAllMembers = CommissionMemberGroupItem.NeedAllMembers;
                OnPropertyChanged(() => e.PropertyName);
            }
            if (e.PropertyName == "Stage" && Stage != CommissionMemberGroupItem.Stage)
            {
                Stage = CommissionMemberGroupItem.Stage;
                OnPropertyChanged(() => e.PropertyName);
            }
            //OnPropertyChanged(() => CommissionMemberGroupItem);
        }

        public async Task InitializeNew(int commissionMemberId, int stage, int commissionProtocolId)
        {
            ChangeTracker.IsEnabled = false;
            EraseProperties();
            var commMemberId = commissionMemberId.ToInt();
            if (commMemberId < 1)
                return;

            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            CommissionMemberId = commMemberId;
            this.commissionProtocolId = commissionProtocolId;
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Загрузка нового решения протокола комиссии...");
            logService.InfoFormat("Loading сommission member with id={0} and stage={1}...", commMemberId, stage);
            IDisposableQueryable<CommissionMember> commissionMemberQuery = commissionService.GetCommissionMember(commMemberId);
            IDisposableQueryable<CommissionProtocol> commissionProtocolQuery = commissionService.GetCommissionProtocolById(commissionProtocolId);
            try
            {
                Stage = stage;
                CommissionDecisionId = SpecialValues.NewId;
                var commissionDecision = await commissionMemberQuery.Select(x => new
                {
                    MemberName = x.PersonStaffId.HasValue ? x.PersonStaff.Staff.ShortName + " - " + x.PersonStaff.Person.ShortName : x.StaffId.HasValue ? x.Staff.ShortName : string.Empty,
                })
                .FirstOrDefaultAsync(token);
                MemberName = commissionDecision.MemberName;

                var commissionProtocolIsCompleted = await commissionProtocolQuery.Select(x => x.IsCompleted).FirstOrDefaultAsync(token);
                CanDeleteMember = commissionProtocolIsCompleted != true || securityService.HasPermission(Permission.DeleteCommissionDecisionWithDecision);

                NeedAllMembers = await commissionProtocolQuery.SelectMany(x => x.CommissionDecisions.Where(y => y.CommissionStage == stage)).Select(x => x.NeedAllMembersInStage).FirstOrDefaultAsync(token);

                loadingIsCompleted = true;
                ChangeTracker.IsEnabled = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load сommissionmember with id={0} and stage={1}...", commMemberId, stage);
                loadingIsCompleted = true;
            }
            finally
            {
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (commissionMemberQuery != null)
                {
                    commissionMemberQuery.Dispose();
                }
            }
        }

        public async Task Initialize(int? commissionDecisionId)
        {
            ChangeTracker.IsEnabled = false;
            var commDecisionId = commissionDecisionId.ToInt();
            EraseProperties();
            if (commDecisionId < 1)
                return;

            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            CommissionDecisionId = commDecisionId;
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Загрузка решения протокола комиссии...");
            logService.InfoFormat("Loading сommission decision with id={0}...", commDecisionId);
            IDisposableQueryable<CommissionDecision> commissionDecisionsQuery = null;
            try
            {
                commissionDecisionsQuery = commissionService.GetCommissionDecision(commDecisionId);
                var commissionDecision = await commissionDecisionsQuery.Select(x => new
                {
                    DecisionDate = x.DecisionDateTime,
                    Stage = x.CommissionStage,
                    MemberName = x.CommissionMember.PersonStaffId.HasValue ? x.CommissionMember.PersonStaff.Staff.ShortName + " - " + x.CommissionMember.PersonStaff.Person.ShortName :
                        x.CommissionMember.StaffId.HasValue ? x.CommissionMember.Staff.ShortName : string.Empty,
                    DecisionName = x.DecisionId.HasValue ? x.Decision.Name : string.Empty,
                    x.Comment,
                    ColorType = x.DecisionId.HasValue && x.Decision.DecisionKindId.HasValue ? x.Decision.DecisionKind.ColorsSetting.Hex : emptyDecisionColor,
                    x.NeedAllMembersInStage,
                    x.CommissionProtocolId,
                    x.CommissionMemberId,
                    x.IsOfficial,
                    CommissionIsCompleted = x.CommissionProtocol.IsCompleted
                }).FirstOrDefaultAsync(token);
                CanDeleteMember = (commissionDecision.CommissionIsCompleted != true && commissionDecision.DecisionName == string.Empty) || securityService.HasPermission(Permission.DeleteCommissionDecisionWithDecision);
                CanChangeIsOfficial = commissionDecision.DecisionName != string.Empty && securityService.HasPermission(Permission.CanChangeCommissionDecisionIsOfficial);
                commissionProtocolId = commissionDecision.CommissionProtocolId;
                DecisionDate = commissionDecision.DecisionDate;
                Stage = commissionDecision.Stage;
                NeedAllMembers = commissionDecision.NeedAllMembersInStage;
                IsOfficial = commissionDecision.IsOfficial;
                MemberName = commissionDecision.MemberName;
                CommissionMemberId = commissionDecision.CommissionMemberId;
                Decision = commissionDecision.DecisionName;
                ColorType = commissionDecision.ColorType;
                Comment = commissionDecision.Comment;
                this.OnPropertyChanged();
                loadingIsCompleted = true;
                ChangeTracker.IsEnabled = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load сommission decision with id={0}...", commDecisionId);
                loadingIsCompleted = true;
            }
            finally
            {
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (commissionDecisionsQuery != null)
                {
                    commissionDecisionsQuery.Dispose();
                }
            }
        }
        #endregion
    }
}
