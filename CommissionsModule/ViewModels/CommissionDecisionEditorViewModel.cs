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
        #region Constructors
        public CommissionDecisionEditorViewModel()
        {
            Decisions = new ObservableCollectionEx<Decision>();
        }
        #endregion

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

        private bool needDecisionDateTime;
        public bool NeedDecisionDateTime
        {
            get { return needDecisionDateTime; }
            set { SetTrackedProperty(ref needDecisionDateTime, value); }
        }

        private DateTime? decisionDateTime;
        public DateTime? DecisionDateTime
        {
            get { return decisionDateTime; }
            set { SetTrackedProperty(ref decisionDateTime, value); }
        }

        #endregion

        #region Methods

        public void Initialize(string comment, DateTime? decisionDateTime, Decision selectedDecision, IEnumerable<Decision> decisions)
        {
            SelectedDecision = null;
            Comment = comment;
            DecisionDateTime = decisionDateTime;
            NeedDecisionDateTime = decisionDateTime.HasValue;
            Decisions.Clear();
            Decisions.AddRange(decisions);
            SelectedDecision = selectedDecision;
        }

        #endregion
    }
}
