using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using DataLib;
using Core;
using log4net;
using GalaSoft.MvvmLight;
using MainLib;

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
            this.SelectMembersToPreviousStageCommand = new RelayCommand(SelectMembersToPreviousStage);
            this.SelectMembersToNextStageCommand = new RelayCommand(SelectMembersToNextStage);
            this.SaveDecisionCommand = new RelayCommand(SaveDecision);
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
                CommissionDecisions.Add(ConvertToCommisionDecisionDTO(item));

            // ?????????????????
            AllowSave = commissionDecisions.Where(x => x.CommissionStage < myLastCommissionDecision.CommissionStage).All(x => x.DecisionId.HasValue)
                        && !commissionDecisions.Any(x => x.CommissionStage > myLastCommissionDecision.CommissionStage && x.DecisionId.HasValue);
        }

        private CommissionDecisionDTO ConvertToCommisionDecisionDTO(CommissionDecision item)
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

        private string previousMembers;
        public string PreviousMembers
        {
            get { return previousMembers; }
            set { Set("PreviousMembers", ref previousMembers, value); }
        }

        private string nextMembers;
        public string NextMembers
        {
            get { return nextMembers; }
            set { Set("NextMembers", ref nextMembers, value); }
        }

        private RelayCommand selectMembersToPreviousStageCommand;
        public RelayCommand SelectMembersToPreviousStageCommand
        {
            get { return selectMembersToPreviousStageCommand; }
            set { Set("SelectMembersToPreviousStageCommand", ref selectMembersToPreviousStageCommand, value); }
        }

        private RelayCommand selectMembersToNextStageCommand;
        public RelayCommand SelectMembersToNextStageCommand
        {
            get { return selectMembersToNextStageCommand; }
            set { Set("SelectMembersToNextStageCommand", ref selectMembersToNextStageCommand, value); }
        }  

        private RelayCommand saveDecisionCommand;
        public RelayCommand SaveDecisionCommand
        {
            get { return saveDecisionCommand; }
            set { Set("SaveDecisionCommand", ref saveDecisionCommand, value); }
        }

        private void SelectMembersToPreviousStage()
        {
            SelectCommissionMembersViewModel selectedMembers = GetSelectedCommissionMembers();
            if (selectedMembers == null || !selectedMembers.resultPersonStaffs.Any()) return;
            
            foreach (var commissionDecisionId in commissionService.GetCommissionDecisionsByProtocolId(this.commissionProtocolId).Where(x => x.CommissionStage >= myLastCommissionDecision.CommissionStage).Select(x => x.Id))
            {
                CommissionDecision commissionDecision = commissionService.GetCommissionDecisionById(commissionDecisionId);
                commissionDecision.CommissionStage = commissionDecision.CommissionStage + 1;
                string message = string.Empty;
                if (commissionService.Save(commissionDecision, out message) == 0)
                {
                    dialogService.ShowError("При сохранении возникла ошибка: " + message);
                    log.Error(string.Format("Failed to update CommissionStage for CommissionDecision." + message));
                    return;
                }
                CommissionDecisions.First(x => x.Id == commissionDecisionId).Stage = commissionDecision.CommissionStage;
                CommissionDecisions.First(x => x.Id == commissionDecisionId).StageText = "Этап " + commissionDecision.CommissionStage + " - ";
            }

            foreach (var personStaff in selectedMembers.resultPersonStaffs)
            {
                var commissionDecisionId = CreateCommissionDecision(personStaff, myLastCommissionDecision.CommissionStage);
                CommissionDecisions.Add(ConvertToCommisionDecisionDTO(commissionService.GetCommissionDecisionById(commissionDecisionId)));
                CommissionDecisions = new ObservableCollection<CommissionDecisionDTO>(CommissionDecisions.OrderBy(x => x.Stage));
            }            
        }
                    
        private void SelectMembersToNextStage()
        {
            SelectCommissionMembersViewModel selectedMembers = GetSelectedCommissionMembers();
            if (selectedMembers == null || !selectedMembers.resultPersonStaffs.Any()) return;

            foreach (var personStaff in GetSelectedCommissionMembers().resultPersonStaffs)
            {
                var commissionDecisionId = CreateCommissionDecision(personStaff, myLastCommissionDecision.CommissionStage);
                CommissionDecisions.Add(ConvertToCommisionDecisionDTO(commissionService.GetCommissionDecisionById(commissionDecisionId)));
                CommissionDecisions = new ObservableCollection<CommissionDecisionDTO>(CommissionDecisions.OrderBy(x => x.Stage));
            }  
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
            commissionDecision.CommissionStage = (stage == 0 ? 1 : stage);
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
          
        private SelectCommissionMembersViewModel GetSelectedCommissionMembers()
        {
            if (commissionProtocolId == 0) return null;
            var selectMembersModelView = new SelectCommissionMembersViewModel(commissionProtocolBeginDateTime, commissionProtocolEndDateTime, commissionService, personService, dialogService, log);
            var dialogResult = dialogService.ShowDialog(selectMembersModelView);
            if (dialogResult != true)
                return null;
            return selectMembersModelView;
        }

        private void SaveDecision()
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

