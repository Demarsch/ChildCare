using Core.Data.Misc;
using Core.Wpf.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Shared.Commissions.Events;
using Shared.Patient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Shared.Commissions.ViewModels
{
    public class AssignmentCommissionViewModel : BindableBase
    {
        private readonly IDialogServiceAsync dialogService;
        private readonly Func<PersonSearchDialogViewModel> patientSearchFactory;
        private readonly Func<CreateAssignmentCommissionViewModel> createAssignmentCommissionFactory;
        private readonly IEventAggregator eventAggregator; 

        public AssignmentCommissionViewModel(IDialogServiceAsync dialogService, IEventAggregator eventAggregator, Func<PersonSearchDialogViewModel> patientSearchFactory, 
                                                                                Func<CreateAssignmentCommissionViewModel> createAssignmentCommissionFactory)
        {
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (patientSearchFactory == null)
            {
                throw new ArgumentNullException("patientSearchFactory");
            }
            if (createAssignmentCommissionFactory == null)
            {
                throw new ArgumentNullException("createAssignmentCommissionFactory");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            this.patientSearchFactory = patientSearchFactory;
            this.createAssignmentCommissionFactory = createAssignmentCommissionFactory;

            assignCommand = new DelegateCommand(CreateAssignmentToCommission);
        }

        #region Properties

        public int PersonId { get; set; }

        private readonly DelegateCommand assignCommand;
        public ICommand AssignCommand { get { return assignCommand; } }

        #endregion
        
        private async void CreateAssignmentToCommission()
        {
            int currentPersonId = PersonId;
            if (SpecialValues.IsNewOrNonExisting(PersonId)) 
            {
                using (var searchViewModel = patientSearchFactory())
                {
                    var searchDialog = await dialogService.ShowDialogAsync(searchViewModel);
                    if (searchDialog != true)
                        return;
                    currentPersonId = searchViewModel.PersonSearchViewModel.SelectedPersonId;
                }  
            }

            var createAssignmentCommissionViewModel = createAssignmentCommissionFactory();
            createAssignmentCommissionViewModel.Initialize(currentPersonId);
            var dialog = await dialogService.ShowDialogAsync(createAssignmentCommissionViewModel);

            if (dialog == true)
                eventAggregator.GetEvent<CommissionChangedEvent>().Publish(createAssignmentCommissionViewModel.ProtocolId);
        }

        


    }
}
