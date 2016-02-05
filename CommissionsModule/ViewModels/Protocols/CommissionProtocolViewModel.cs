using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommissionsModule.ViewModels
{
    public class CommissionProtocolViewModel : BindableBase
    {
        #region Constructiors
        public CommissionProtocolViewModel(PreliminaryProtocolViewModel preliminaryProtocolViewModel, CommissionСonductViewModel commissionСonductViewModel, CommissionСonclusionViewModel commissionСonclusionViewModel)
        {
            if (preliminaryProtocolViewModel == null)
            {
                throw new ArgumentNullException("preliminaryProtocolViewModel");
            }
            if (commissionСonductViewModel == null)
            {
                throw new ArgumentNullException("commissionСonductViewModel");
            }
            if (commissionСonclusionViewModel == null)
            {
                throw new ArgumentNullException("commissionСonclusionViewModel");
            }
            PreliminaryProtocolViewModel = preliminaryProtocolViewModel;
            CommissionСonductViewModel = commissionСonductViewModel;
            CommissionСonclusionViewModel = commissionСonclusionViewModel;
        }
        #endregion

        #region Properties

        public PreliminaryProtocolViewModel PreliminaryProtocolViewModel { get; set; }
        public CommissionСonductViewModel CommissionСonductViewModel { get; set; }
        public CommissionСonclusionViewModel CommissionСonclusionViewModel { get; set; }

        #endregion
    }
}
