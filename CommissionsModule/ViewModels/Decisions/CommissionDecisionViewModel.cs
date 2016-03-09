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

namespace CommissionsModule.ViewModels
{
    public class CommissionDecisionViewModel : BindableBase
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly ISecurityService securityService;
        private readonly ILog logService;

        private CommandWrapper reloadInitialzeCommandWrapper;

        private CancellationTokenSource currentOperationToken;
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
                SetProperty(ref stage, value);
                if (CommissionMemberGroupItem != null)
                    CommissionMemberGroupItem.Stage = stage;
            }
        }

        private bool needAllMembers;
        public bool NeedAllMembers
        {
            get { return needAllMembers; }
            set
            {
                SetProperty(ref needAllMembers, value);
                if (CommissionMemberGroupItem != null)
                    CommissionMemberGroupItem.NeedAllMembers = needAllMembers;
            }
        }

        private CommissionMemberGroupItem commissionMemberGroupItem;
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

        public bool CanDeleteMember { get; private set; }

        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; set; }
        #endregion

        #region Methods

        public void EraseProperties()
        {
            CommissionMemberGroupItem = new CommissionMemberGroupItem();
            DecisionDate = DateTime.Now;
            Stage = 0;
            NeedAllMembers = false;
            MemberName = string.Empty;
            Decision = string.Empty;
            ColorType = "#E5E5E5";
            Comment = string.Empty;
        }

        public async Task InitializeNew(int commissionMemberId, int stage, bool canDeleteMember)
        {
            var commMemberId = commissionDecisionId.ToInt();
            CanDeleteMember = canDeleteMember && (canDeleteMember || securityService.HasPermission(Permission.DeleteCommissionDecisionWithDecision));
            if (commMemberId < 1)
                return;

            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            CommissionMemberId = commMemberId;
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Загрузка нового решения протокола комиссии...");
            logService.InfoFormat("Loading сommission member with id={0} and stage={1}...", commMemberId, stage);
            IDisposableQueryable<CommissionMember> commissionDecisionsQuery = null;
            try
            {
                commissionDecisionsQuery = commissionService.GetCommissionMember(commMemberId);
                var commissionDecision = await commissionDecisionsQuery.Select(x => new
                {
                    MemberName = x.PersonStaffId.HasValue ? x.PersonStaff.Staff.ShortName + " - " + x.PersonStaff.Person.ShortName : x.StaffId.HasValue ? x.Staff.ShortName : string.Empty
                }).FirstOrDefaultAsync(token);
                MemberName = commissionDecision.MemberName;
                Stage = stage;
                CommissionDecisionId = SpecialValues.NewId;
                loadingIsCompleted = true;
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
                if (commissionDecisionsQuery != null)
                {
                    commissionDecisionsQuery.Dispose();
                }
            }
        }

        public async Task Initialize(int? commissionDecisionId, bool canDeleteMember)
        {
            var commDecisionId = commissionDecisionId.ToInt();
            EraseProperties();
            CanDeleteMember = canDeleteMember && (canDeleteMember || securityService.HasPermission(Permission.DeleteCommissionDecisionWithDecision));
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
                    ColorType = x.DecisionId.HasValue && x.Decision.ColorSettingsId.HasValue ? x.Decision.ColorsSetting.Hex : "#E5E5E5",
                    x.NeedAlllMemmbersInStage
                }).FirstOrDefaultAsync(token);
                DecisionDate = commissionDecision.DecisionDate;
                Stage = commissionDecision.Stage;
                NeedAllMembers = commissionDecision.NeedAlllMemmbersInStage;
                MemberName = commissionDecision.MemberName;
                Decision = commissionDecision.DecisionName;
                ColorType = commissionDecision.ColorType;
                Comment = commissionDecision.Comment;
                loadingIsCompleted = true;
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
