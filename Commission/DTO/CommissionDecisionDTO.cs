using System;
using GalaSoft.MvvmLight;

namespace Commission
{
    public class CommissionDecisionDTO : ObservableObject
    {
        public int Id { get; set; }

        private int stage;
        public int Stage
        {
            get { return stage; }
            set { Set("Stage", ref stage, value); }
        }

        private int memberPersonId;
        public int MemberPersonId
        {
            get { return memberPersonId; }
            set { Set("MemberPersonId", ref memberPersonId, value); }
        }

        private int? decisionId;
        public int? DecisionId
        {
            get { return decisionId; }
            set { Set("DecisionId", ref decisionId, value); }
        }

        private int? decisionParentId;
        public int? DecisionParentId
        {
            get { return decisionParentId; }
            set { Set("DecisionParentId", ref decisionParentId, value); }
        }

        private DateTime? decisionDate;
        public DateTime? DecisionDate
        {
            get { return decisionDate; }
            set { Set("DecisionDate", ref decisionDate, value); }
        }

        private string stageText;
        public string StageText
        {
            get { return stageText; }
            set { Set("StageText", ref stageText, value); }
        }

        private string decisionText;
        public string DecisionText
        {
            get { return decisionText; }
            set { Set("DecisionText", ref decisionText, value); }
        }

        private string memberStaff;
        public string MemberStaff
        {
            get { return memberStaff; }
            set { Set("MemberStaff", ref memberStaff, value); }
        }

        private string memberPersonName;
        public string MemberPersonName
        {
            get { return memberPersonName; }
            set { Set("MemberPersonName", ref memberPersonName, value); }
        }

        private string comment;
        public string Comment
        {
            get { return comment; }
            set { Set("Comment", ref comment, value); }
        }

        private string decisionDateText;
        public string DecisionDateText
        {
            get { return decisionDateText; }
            set { Set("DecisionDateText", ref decisionDateText, value); }
        }

        private bool hasDecision;
        public bool HasDecision
        {
            get { return hasDecision; }
            set { Set("HasDecision", ref hasDecision, value); }
        }
    }
}
