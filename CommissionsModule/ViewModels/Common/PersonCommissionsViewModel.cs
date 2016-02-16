using CommissionsModule.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Navigation;
using Core.Data;
using Core.Data.Classes;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;
using Prism.Events;
using Prism.Regions;
using System.Windows.Media;

namespace CommissionsModule.ViewModels.Common
{
    public class PersonCommissionsViewModel : BindableBase
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly ILog logService;
        private readonly IDialogService messageService;
        private readonly IEventAggregator eventAggregator;
        #endregion

        #region  Constructors
        public PersonCommissionsViewModel(ICommissionService commissionService, ILog logService, IDialogService messageService, IEventAggregator eventAggregator)
        {
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }          
            this.eventAggregator = eventAggregator;
            this.commissionService = commissionService;
            this.logService = logService;
            this.messageService = messageService;

            Commissions = new ObservableCollectionEx<CommissionItemViewModel>();
            BusyMediator = new BusyMediator();
        }
        #endregion

        #region Properties
        public BusyMediator BusyMediator { get; set; }      

        public ObservableCollectionEx<CommissionItemViewModel> Commissions { get; set; }

        private CommissionItemViewModel selectedCommission;
        public CommissionItemViewModel SelectedCommission
        {
            get { return selectedCommission; }
            set 
            {
                SetProperty(ref selectedCommission, value);
                //if (SetProperty(ref selectedCommission, value) && value != null)
                //   eventAggregator.GetEvent<PubSubEvent<int>>().Publish(value.Id);   
            }
        }

        private int personId;
        public int PersonId
        {
            get { return personId; }
            set
            {
                if (SetProperty(ref personId, value) && !SpecialValues.IsNewOrNonExisting(value))
                    LoadCommissionProtocolsAsync();
            }
        }

        #endregion

        #region Methods
        
        public async void LoadCommissionProtocolsAsync()
        {
            BusyMediator.Activate("Загрузка протоколов пациента...");
            logService.Info("Loading person commission protocols...");
            Commissions.Clear();
            IDisposableQueryable<CommissionProtocol> commissionProtocolsQuery = null;
            try
            {
                commissionProtocolsQuery = commissionService.GetPersonCommissionProtocols(PersonId);

                var commissionProtocolsSelectQuery = await commissionProtocolsQuery.Select(x => new
                    {
                        Id = x.Id,
                        Number = x.ProtocolNumber,
                        ProtocolDate = x.ProtocolDate,
                        Talon = x.PersonTalon != null ? new { TalonNumber = x.PersonTalon.TalonNumber, TalonDate = x.PersonTalon.TalonDateTime } : null,
                        MKB = x.MKB,
                        HelpType = x.MedicalHelpTypeId.HasValue ? x.MedicalHelpType.Code : string.Empty,
                        IsCompleted = x.IsCompleted,
                        DecisionId = x.DecisionId,
                        DecisionText = x.Decision.Name
                    }).ToArrayAsync();

                var result = commissionProtocolsSelectQuery.Select(x => new CommissionItemViewModel()
                    {
                        Id = x.Id,
                        ProtocolNumber = x.Number > 0 ? (x.Number + " от " + x.ProtocolDate.ToShortDateString() + " - ") : ("(расм. " + x.ProtocolDate.ToShortDateString() + ") - "),
                        Talon = x.Talon != null ? x.Talon.TalonNumber + " от " + x.Talon.TalonDate.ToShortDateString() : "талон отсутствует",                       
                        MKB = !string.IsNullOrEmpty(x.MKB) ? "МКБ: " + x.MKB : string.Empty,
                        MedHelpType = x.HelpType,
                        IsCompleted = x.IsCompleted,
                        DecisionText = x.DecisionText,
                        DecisionColorHex = commissionService.GetDecisionColorHex(x.DecisionId)
                    }).ToArray();

                Commissions.AddRange(result);
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load commission protocols");
                messageService.ShowError("Не удалось загрузить протоколы комиссий пациента. ");
            }
            finally
            {
                if (commissionProtocolsQuery != null)
                    commissionProtocolsQuery.Dispose();
                BusyMediator.Deactivate();
            }
        }

        #endregion             
    }
}
