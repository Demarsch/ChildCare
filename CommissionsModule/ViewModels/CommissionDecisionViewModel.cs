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
        private DateTime decisionDate;
        public DateTime DecisionDate
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

        private int colorType;
        public int ColorType
        {
            get { return colorType; }
            set { SetProperty(ref colorType, value); }
        }
        #endregion
    }
}
