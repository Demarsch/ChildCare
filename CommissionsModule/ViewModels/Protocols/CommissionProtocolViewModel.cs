using Core.Data.Misc;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommissionsModule.ViewModels
{
    public class CommissionProtocolViewModel : BindableBase, IDisposable
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;
        #endregion

        #region Constructiors
        public CommissionProtocolViewModel(IEventAggregator eventAggregator, PreliminaryProtocolViewModel preliminaryProtocolViewModel, CommissionСonductViewModel commissionСonductViewModel, CommissionСonclusionViewModel commissionСonclusionViewModel)
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
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregatorСonclusionViewModel");
            }
            this.eventAggregator = eventAggregator;
            PreliminaryProtocolViewModel = preliminaryProtocolViewModel;
            CommissionСonductViewModel = commissionСonductViewModel;
            CommissionСonclusionViewModel = commissionСonclusionViewModel;

            SubscribeToEvents();
        }

       
        #endregion

        #region Properties

        public PreliminaryProtocolViewModel PreliminaryProtocolViewModel { get; set; }
        public CommissionСonductViewModel CommissionСonductViewModel { get; set; }
        public CommissionСonclusionViewModel CommissionСonclusionViewModel { get; set; }

        private int selectedCommissionProtocolId = 0;
        public int SelectedCommissionProtocolId
        {
            get { return selectedCommissionProtocolId; }
            set
            {
                if (SetProperty(ref selectedCommissionProtocolId, value))
                {
                    if (PreliminaryProtocolViewModel != null)
                        PreliminaryProtocolViewModel.Initialize(SelectedCommissionProtocolId);
                }
            }
        }
        #endregion

        #region Methods

        public void Dispose()
        {
            UnsubscriveFromEvents();
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<PubSubEvent<int>>().Unsubscribe(OnCommissionProtocolSelected);
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<PubSubEvent<int>>().Subscribe(OnCommissionProtocolSelected);
        }

        private void OnCommissionProtocolSelected(int protocolId)
        {
            SelectedCommissionProtocolId = 0;
            if (!SpecialValues.IsNewOrNonExisting(protocolId))
                SelectedCommissionProtocolId = protocolId;
        }

        #endregion
    }
}
