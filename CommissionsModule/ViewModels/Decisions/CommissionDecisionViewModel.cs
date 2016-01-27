using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommissionsModule.ViewModels
{
    public class CommissionDecisionViewModel : BindableBase
    {
        #region Constructors
        public CommissionDecisionViewModel()
        {
            int i = 0;
        }
        #endregion

        #region Properties
        private int commissionDecisionId = 0;
        public int CommissionDecisionId
        {
            get { return commissionDecisionId; }
            set { SetProperty(ref commissionDecisionId, value); }
        }

        private int stage = 0;
        public int Stage
        {
            get { return stage; }
            set
            {
                SetProperty(ref stage, value);
                OnPropertyChanged(() => StageText);
            }
        }

        public string StageText
        {
            get { return Stage + "-й этап"; }
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

        private string colorType;
        public string ColorType
        {
            get { return colorType; }
            set { SetProperty(ref colorType, value); }
        }
        #endregion

        #region Methods

        public void Initialize(int commissionDecisionId, DateTime? decisionDate, int stage, string memberName, string decisionName, string colorType)
        {
            CommissionDecisionId = commissionDecisionId;
            DecisionDate = decisionDate;
            Stage = stage;
            MemberName = memberName;
            Decision = decisionName;
            ColorType = colorType;
        }

        #endregion
    }
}
