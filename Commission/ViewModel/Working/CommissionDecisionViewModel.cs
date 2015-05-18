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
        private IPersonService personService;
        private IDialogService dialogService;
        private ILog log;
        private User currentUser;
        private int commissionProtocolId = 0;

        public CommissionDecisionViewModel(CommissionService commissionService, UserService userService, PersonService personService,
            IDialogService dialogService, ILog log)
        {
            this.commissionService = commissionService;
            this.userService = userService;
            this.personService = personService;
            this.dialogService = dialogService;
            this.log = log;

            this.currentUser = userService.GetCurrentUser();
            this.SaveDecisionCommand = new RelayCommand<object>(SaveDecision);
        }

        public void Load(int CommissionProtocolId)
        {
            commissionProtocolId = CommissionProtocolId;
            
            MainDecisions = new ObservableCollection<Decision>(commissionService.GetActualMainDecisions());
            LoadCommissionDecisionByDefault();
            CommissionDecisions = new ObservableCollection<CommissionDecisionDTO>();

            foreach (var item in commissionService.GetCommissionDecisionsByProtocolId(commissionProtocolId))            
                CommissionDecisions.Add(ConvertToCommisionDecisionDTO(item));
        }

        private CommissionDecisionDTO ConvertToCommisionDecisionDTO(CommissionDecision item)
        {
            return new CommissionDecisionDTO() {
                                            Id = item.Id,
                                            MemberPersonId = commissionService.GetCommissionMemberPersonById(item.CommissionMemberId).Id,
                                            MemberStaff = commissionService.GetCommissionMemberStaffById(item.CommissionMemberId).Name,
                                            MemberPersonName = commissionService.GetCommissionMemberPersonById(item.CommissionMemberId).ShortName + ": ",
                                            Stage = item.CommissionStage,
                                            StageText = "Этап " + item.CommissionStage + " - ",
                                            DecisionId = item.DecisionId,
                                            DecisionParentId = (item.DecisionId.HasValue ? commissionService.GetDecisionById(item.DecisionId.Value).ParentId : (int?)null),
                                            DecisionText = (item.DecisionId.HasValue ? commissionService.GetDecisionNameById(item.DecisionId.Value) : string.Empty),
                                            DecisionDate = item.DecisionInDateTime,
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

        private CommissionDecisionDTO selectedCommissionDecision;
        public CommissionDecisionDTO SelectedCommissionDecision
        {
            get { return selectedCommissionDecision; }
            set
            {
                if (!Set("SelectedCommissionDecision", ref selectedCommissionDecision, value) || value == null)
                    return;

                if (value.MemberPersonId == this.currentUser.PersonId)
                    LoadMyCommisionDecision(value);
                else
                    LoadCommissionDecisionByDefault();
            }
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
                SpecificDecisions = new ObservableCollection<Decision>(commissionService.GetActualSpecificDecisions(value.Id));
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

        private RelayCommand<object> saveDecisionCommand;
        public RelayCommand<object> SaveDecisionCommand
        {
            get { return saveDecisionCommand; }
            set { Set("SaveDecisionCommand", ref saveDecisionCommand, value); }
        }

        private void LoadMyCommisionDecision(CommissionDecisionDTO myCommissionDecision)
        {
            if (myCommissionDecision.DecisionId.HasValue)
            {
                SelectedMainDecision = MainDecisions.FirstOrDefault(x => x.Id == myCommissionDecision.DecisionParentId);
                SelectedSpecificDecision = SpecificDecisions.FirstOrDefault(x => x.Id == myCommissionDecision.DecisionId.Value);
            }
            Comment = myCommissionDecision.Comment;
        }
        
        private void LoadCommissionDecisionByDefault()
        {
            SelectedMainDecision = mainDecisions.FirstOrDefault();
            SelectedSpecificDecision = specificDecisions.FirstOrDefault();
            Comment = string.Empty;
        }

        private void SaveDecision(object parameter)
        {
            CommissionDecision decision = null;

            if (SelectedCommissionDecision.MemberPersonId == currentUser.PersonId)
                decision = commissionService.GetCommissionDecisionById(SelectedCommissionDecision.Id);
            else
                decision = new CommissionDecision();
            decision.CommissionProtocolId = this.commissionProtocolId;
            decision.CommissionMemberId = 1;// ?????
            decision.IsOfficial = true; //?????
            decision.DecisionId = selectedSpecificDecision.Id;
            decision.DecisionInDateTime = DateTime.Now; //????
            decision.Comment = comment;
            decision.CommissionStage = 1; // ??????
            decision.InitiatorMemberId = 1; //??????
            decision.InDateTime = DateTime.Now; //?????

            string message = string.Empty;
            if (commissionService.Save(decision, out message))
            {
                dialogService.ShowMessage("Данные сохранены");
                if (CommissionDecisions.Any(x => x.Id == decision.Id))
                {
                    var dto = ConvertToCommisionDecisionDTO(decision);
                    SelectedCommissionDecision.StageText = dto.StageText;
                    SelectedCommissionDecision.MemberStaff = dto.MemberStaff;
                    SelectedCommissionDecision.MemberPersonName = dto.MemberPersonName;
                    SelectedCommissionDecision.DecisionText = dto.DecisionText;
                    SelectedCommissionDecision.Comment = dto.Comment;
                    SelectedCommissionDecision.DecisionDateText = dto.DecisionDateText;
                }
                else
                    CommissionDecisions.Add(ConvertToCommisionDecisionDTO(decision));
            }
            else
            {
                dialogService.ShowError("При сохранении возникла ошибка: " + message);
                log.Error(string.Format("Failed to Save user. " + message));
            }            
        }
    }

}

