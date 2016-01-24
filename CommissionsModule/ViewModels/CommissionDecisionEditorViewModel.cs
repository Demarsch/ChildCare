using Core.Data;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommissionsModule.ViewModels
{
    public class CommissionDecisionEditorViewModel : TrackableBindableBase
    {
        #region Properties
        public ObservableCollectionEx<Decision> Decisions { get; set; }

        private Decision selectedDecision;
        public Decision SelectedDecision
        {
            get { return selectedDecision; }
            set { SetTrackedProperty(ref selectedDecision, value); }
        }

        private string comment;
        public string Comment
        {
            get { return comment; }
            set { SetTrackedProperty(ref comment, value); }
        }

        private bool needDecisionInDateTime;
        public bool NeedDecisionInDateTime
        {
            get { return needDecisionInDateTime; }
            set { SetTrackedProperty(ref needDecisionInDateTime, value); }
        }

        private DateTime decisionInDateTime;
        public DateTime DecisionInDateTime
        {
            get { return decisionInDateTime; }
            set { SetTrackedProperty(ref decisionInDateTime, value); }
        }

        #endregion
    }
}
