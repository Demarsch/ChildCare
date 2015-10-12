using System;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight.CommandWpf;
using DataLib;
using Core;
using log4net;
using GalaSoft.MvvmLight;

namespace Commission
{   
    [PropertyChanged.ImplementPropertyChanged]
    public class CommissionDecisionViewModel : ViewModelBase
    {
        private ICommissionService commissionService;
        private IUserService userService;
        private IUserSystemInfoService userSystemInfoService;
        private IPersonService personService;
        private IDialogService dialogService;
        private ILog log;

        private User currentUser;
        private DateTime commissionProtocolBeginDateTime;
        private DateTime commissionProtocolEndDateTime;
        private CommissionDecision myLastCommissionDecision;
        private int commissionProtocolId;

        public CommissionDecisionViewModel(ICommissionService commissionService, IUserService userService, IUserSystemInfoService userSystemInfoService, IPersonService personService,
            IDialogService dialogService, ILog log)
        {
            this.commissionService = commissionService;
            this.userService = userService;
            this.userSystemInfoService = userSystemInfoService;
            this.personService = personService;
            this.dialogService = dialogService;
            this.log = log;

            this.currentUser = userService.GetCurrentUser(userSystemInfoService);
            this.RequestMembersCommand = new RelayCommand(RequestMembers);
            this.SaveDecisionCommand = new RelayCommand(SaveCommissionDecision);
        }

        public void Load(int commissionProtocolId)
        {
            this.commissionProtocolId = commissionProtocolId;
            var commissionProtocol = commissionService.GetCommissionProtocolById(this.commissionProtocolId);
            commissionProtocolBeginDateTime = commissionProtocol.ProtocolDate;
            commissionProtocolEndDateTime = commissionProtocol.ProtocolDate;

            CommissionName = commissionService.GetCommissionTypeById(commissionProtocol.CommissionTypeId).Name;
            Talon = (commissionProtocol.PersonTalonId.HasValue ? personService.GetPersonTalonById(commissionProtocol.PersonTalonId.Value).NumberWithDate : string.Empty);

            myLastCommissionDecision = commissionService.GetLastCommissionDecisionByMemberPersonId(commissionProtocolId, this.currentUser.PersonId);

            MainDecisions = new ObservableCollection<Decision>(commissionService.GetActualMainDecisions(commissionProtocolBeginDateTime, commissionProtocolEndDateTime));
            if (myLastCommissionDecision != null && myLastCommissionDecision.DecisionId.HasValue)
            {
                var specificDecision = commissionService.GetDecisionById(myLastCommissionDecision.DecisionId.Value);
                SelectedMainDecision = mainDecisions.FirstOrDefault(x => x.Id == specificDecision.ParentId);
                SelectedSpecificDecision = specificDecisions.FirstOrDefault(x => x.Id == specificDecision.Id);
                Comment = myLastCommissionDecision.Comment;
            }
            else
            {
                SelectedMainDecision = mainDecisions.FirstOrDefault();
                SelectedSpecificDecision = specificDecisions.FirstOrDefault();
                Comment = string.Empty;
            }

            CommissionDecisions = new ObservableCollection<CommissionDecisionDTO>();
            var commissionDecisions = commissionService.GetCommissionDecisionsByProtocolId(commissionProtocolId);
            foreach (var item in commissionDecisions)            
                CommissionDecisions.Add(ConvertToCommissionDecisionDTO(item));

            // ?????????????????
            AllowSave = !commissionDecisions.Any(x => x.CommissionStage > myLastCommissionDecision.CommissionStage && x.DecisionId.HasValue);
        }

        private CommissionDecisionDTO ConvertToCommissionDecisionDTO(CommissionDecision item)
        {
            return new CommissionDecisionDTO() {
                                            Id = item.Id,
                                            MemberStaff = commissionService.GetCommissionMemberStaffById(item.CommissionMemberId).Name,
                                            MemberPersonName = commissionService.GetCommissionMemberPersonById(item.CommissionMemberId).ShortName + ": ",
                                            Stage = item.CommissionStage,
                                            StageText = "Этап " + item.CommissionStage + " - ",
                                            DecisionText = (item.DecisionId.HasValue ? commissionService.GetDecisionNameById(item.DecisionId.Value) : string.Empty),
                                            DecisionDateText = (item.DecisionInDateTime.HasValue ? "Решение от " + item.DecisionInDateTime.Value.ToShortDateString() : "на рассмотрении"),
                                            Comment = string.IsNullOrWhiteSpace(item.Comment) ? "отсутствуют" : item.Comment,
                                            HasDecision = item.DecisionId.HasValue
                                        };
        }

        private ObservableCollection<CommissionDecisionDTO> commissionDecisions;
        public ObservableCollection<CommissionDecisionDTO> CommissionDecisions
        {
            get { return commissionDecisions; }
            set { Set("CommissionDecisions", ref commissionDecisions, value); }
        }        
        
        private ObservableCollection<Decision> mainDecisions;
        public ObservableCollection<Decision> MainDecisions
        {
            get { return mainDecisions; }
            set { Set("MainDecisions", ref mainDecisions, value); }
        }

        private Decision selectedMainDecision;
        public Decision SelectedMainDecision
        {
            get { return selectedMainDecision; }
            set 
            { 
                if (!Set("SelectedMainDecision", ref selectedMainDecision, value) || value == null)
                    return;
                SpecificDecisions = new ObservableCollection<Decision>(commissionService.GetActualSpecificDecisions(value.Id, commissionProtocolBeginDateTime, commissionProtocolEndDateTime));
            }
        }

        private ObservableCollection<Decision> specificDecisions;
        public ObservableCollection<Decision> SpecificDecisions
        {
            get { return specificDecisions; }
            set { Set("SpecificDecisions", ref specificDecisions, value); }
        }

        private Decision selectedSpecificDecision;
        public Decision SelectedSpecificDecision
        {
            get { return selectedSpecificDecision; }
            set { Set("SelectedSpecificDecision", ref selectedSpecificDecision, value); }
        }

        private string comment;
        public string Comment
        {
            get { return comment; }
            set { Set("Comment", ref comment, value); }
        }

        private string commissionName;
        public string CommissionName
        {
            get { return commissionName; }
            set { Set("CommissionName", ref commissionName, value); }
        }

        private string talon;
        public string Talon
        {
            get { return talon; }
            set { Set("Talon", ref talon, value); }
        }

        private bool allowSave;
        public bool AllowSave
        {
            get { return allowSave; }
            set { Set("AllowSave", ref allowSave, value); }
        }

        private string requestedMembersLabel;
        public string RequestedMembersLabel
        {
            get { return requestedMembersLabel; }
            set { Set("RequestedMembersLabel", ref requestedMembersLabel, value); }
        }

        private RelayCommand requestMembersCommand;
        public RelayCommand RequestMembersCommand
        {
            get { return requestMembersCommand; }
            set { Set("RequestMembersCommand", ref requestMembersCommand, value); }
        }  

        private RelayCommand saveDecisionCommand;
        public RelayCommand SaveDecisionCommand
        {
            get { return saveDecisionCommand; }
            set { Set("SaveDecisionCommand", ref saveDecisionCommand, value); }
        }

        private void RequestMembers()        
        {
            SelectCommissionMembersViewModel requestedMembers = GetRequestedCommissionMembers();
            if (requestedMembers == null || !requestedMembers.resultPersonStaffs.Any()) return;

            foreach (var personStaff in requestedMembers.resultPersonStaffs)
            {
                var commissionDecisionId = CreateCommissionDecision(personStaff, myLastCommissionDecision.CommissionStage);
                CommissionDecisions.Add(ConvertToCommissionDecisionDTO(commissionService.GetCommissionDecisionById(commissionDecisionId)));
                CommissionDecisions = new ObservableCollection<CommissionDecisionDTO>(CommissionDecisions.OrderBy(x => x.Stage));
            }

            RequestedMembersLabel += (string.IsNullOrWhiteSpace(RequestedMembersLabel) ? "Запрос отправлен: " : "; ") 
                                        + requestedMembers.resultPersonStaffs.Select(x => personService.GetPersonById(x.PersonId).ShortName).Aggregate((x, y) => x + "; " + y);
        }

        private int CreateCommissionDecision(PersonStaff personStaff, int stage)
        {
            var member = commissionService.GetCommissionMemberById(myLastCommissionDecision.CommissionMemberId);
            CommissionMember commissionMember = new CommissionMember();
            commissionMember.PersonStaffId = personStaff.Id;
            commissionMember.CommissionMemberTypeId = member.CommissionMemberTypeId;
            commissionMember.CommissionTypeId = member.CommissionTypeId;
            commissionMember.BeginDateTime = member.BeginDateTime;
            commissionMember.EndDateTime = member.EndDateTime;
            string message = string.Empty;
            int memberId = commissionService.Save(commissionMember, out message);
            if (memberId == 0)            
            {
                dialogService.ShowError("При сохранении возникла ошибка: " + message);
                log.Error(string.Format("Failed to Save CommissionMember. " + message));
                return 0;
            }

            CommissionDecision commissionDecision = new CommissionDecision();
            commissionDecision.CommissionProtocolId = myLastCommissionDecision.CommissionProtocolId;
            commissionDecision.CommissionMemberId = memberId;
            commissionDecision.IsOfficial = myLastCommissionDecision.IsOfficial;
            commissionDecision.DecisionId = (int?)null;
            commissionDecision.DecisionInDateTime = (DateTime?)null;
            commissionDecision.Comment = string.Empty;
            commissionDecision.CommissionStage = stage;
            commissionDecision.InitiatorMemberId = myLastCommissionDecision.CommissionMemberId;
            commissionDecision.InDateTime = DateTime.Now;
            message = string.Empty;
            int commissionDecisionId = commissionService.Save(commissionDecision, out message);
            if (commissionDecisionId != 0)
                return commissionDecisionId;            
            else
            {
                dialogService.ShowError("При сохранении возникла ошибка: " + message);
                log.Error(string.Format("Failed to Save CommissionDecision. " + message));
                return 0;
            }
        }

        private SelectCommissionMembersViewModel GetRequestedCommissionMembers()
        {
            if (commissionProtocolId == 0) return null;
            var selectMembersModelView = new SelectCommissionMembersViewModel(commissionProtocolBeginDateTime, commissionProtocolEndDateTime, commissionService, personService, dialogService, log);
            var dialogResult = dialogService.ShowDialog(selectMembersModelView);
            if (dialogResult != true)
                return null;
            return selectMembersModelView;
        }

        private void SaveCommissionDecision()
        {
            CommissionDecision commissionDecision = commissionService.GetCommissionDecisionById(myLastCommissionDecision.Id);
            commissionDecision.DecisionId = selectedSpecificDecision.Id;
            commissionDecision.DecisionInDateTime = DateTime.Now;
            commissionDecision.Comment = comment;

            string message = string.Empty;
            if (commissionService.Save(commissionDecision, out message) != 0)
            {
                dialogService.ShowMessage("Данные сохранены");
                var savedDecision = CommissionDecisions.First(x => x.Id == commissionDecision.Id);
                savedDecision.DecisionText = commissionService.GetDecisionNameById(commissionDecision.DecisionId.Value);
                savedDecision.DecisionDateText = "Решение от " + commissionDecision.DecisionInDateTime.Value.ToShortDateString();
                savedDecision.Comment = string.IsNullOrWhiteSpace(commissionDecision.Comment) ? "отсутствуют" : commissionDecision.Comment;                                             
            }
            else
            {
                dialogService.ShowError("При сохранении возникла ошибка: " + message);
                log.Error(string.Format("Failed to Save CommissionDecision. " + message));
            }
        }
    }

}

