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
using PatientInfoModule.Services;
using System.Windows.Input;
using Core.Services;

namespace PatientInfoModule.ViewModels
{
    public class PersonCommissionsCollectionViewModel : BindableBase
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly ILog logService;
        private readonly IDialogService messageService;
        private readonly ISecurityService securityService;
        private readonly IUserService userService;
        private readonly IEventAggregator eventAggregator;
        #endregion

        #region  Constructors
        public PersonCommissionsCollectionViewModel(ICommissionService commissionService, ILog logService, IDialogService messageService,
            ISecurityService securityService, IUserService userService, IEventAggregator eventAggregator)
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
            if (securityService == null)
            {
                throw new ArgumentNullException("securityService");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }          
            this.eventAggregator = eventAggregator;
            this.commissionService = commissionService;
            this.logService = logService;
            this.userService = userService;
            this.securityService = securityService;
            this.messageService = messageService;

            removeCommissionProtocolCommand = new DelegateCommand<int?>(RemoveCommissionProtocol);
            printCommissionProtocolCommand = new DelegateCommand<int?>(PrintCommissionProtocol);

            Commissions = new ObservableCollectionEx<PersonCommissionViewModel>();
            BusyMediator = new BusyMediator();
        }
                
        #endregion

        #region Properties
        public BusyMediator BusyMediator { get; set; }

        private DelegateCommand<int?> removeCommissionProtocolCommand;
        public ICommand RemoveCommissionProtocolCommand { get { return removeCommissionProtocolCommand; } }

        private DelegateCommand<int?> printCommissionProtocolCommand;
        public ICommand PrintCommissionProtocolCommand { get { return printCommissionProtocolCommand; } }

        public ObservableCollectionEx<PersonCommissionViewModel> Commissions { get; set; }

        private PersonCommissionViewModel selectedCommission;
        public PersonCommissionViewModel SelectedCommission
        {
            get { return selectedCommission; }
            set 
            {
                if (SetProperty(ref selectedCommission, value) && value != null)
                {
                    eventAggregator.GetEvent<PubSubEvent<int>>().Publish(value.Id);
                    eventAggregator.GetEvent<PubSubEvent<PersonCommissionViewModel>>().Publish(value);
                }
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
                        ProtocolDate = x.CommissionDate,
                        Talon = x.PersonTalon != null ? new { TalonNumber = x.PersonTalon.TalonNumber, TalonDate = x.PersonTalon.TalonDateTime } : null,
                        MKB = x.MKB,
                        HelpType = x.MedicalHelpTypeId.HasValue ? x.MedicalHelpType.Code : string.Empty,
                        IsCompleted = x.IsCompleted,
                        DecisionId = x.DecisionId,
                        DecisionText = x.Decision.Name
                    }).ToArrayAsync();

                var result = commissionProtocolsSelectQuery.Select(x => new PersonCommissionViewModel()
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

        private async void RemoveCommissionProtocol(int? selectedProtocolId)
        {
            if (SpecialValues.IsNewOrNonExisting(PersonId))
            {
                messageService.ShowWarning("Не выбран пациент.");
                return;
            }
            if (!selectedProtocolId.HasValue || SpecialValues.IsNewOrNonExisting(selectedProtocolId.Value))
            {
                messageService.ShowWarning("Выберите комиссию");
                return;
            }
            if (messageService.AskUser("Удалить комиссию ?") == true)
            {
                if (!securityService.HasPermission(Permission.RemoveCommissionProtocol))
                {
                    messageService.ShowWarning("У вас нет прав на удаление комиссии.");
                    return;
                }
                var deletedCommission = commissionService.GetCommissionProtocolById(selectedProtocolId.Value).FirstOrDefault();
                if (deletedCommission != null && deletedCommission.InUserId != userService.GetCurrentUser().Id)
                {
                    messageService.ShowWarning("Комиссию может удалить только тот, кто ее создал.");
                    return;
                }
                bool isOk = await commissionService.RemoveCommissionProtocol(deletedCommission.Id);
                if (isOk)
                    LoadCommissionProtocolsAsync();
            }
        }

        private void PrintCommissionProtocol(int? selectedProtocolId)
        {
            if (SpecialValues.IsNewOrNonExisting(PersonId))
            {
                messageService.ShowWarning("Не выбран пациент.");
                return;
            }
            if (!selectedProtocolId.HasValue || SpecialValues.IsNewOrNonExisting(selectedProtocolId.Value))
            {
                messageService.ShowWarning("Выберите комиссию");
                return;
            }
            messageService.ShowWarning("Отсутствует печатная форма протокола");
        }

        #endregion             
    }
}
