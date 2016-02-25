using Core.Data.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using PatientInfoModule.Misc;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace PatientInfoModule.ViewModels
{
    public class PersonTalonsCommissionsHospitalisationsViewModel : BindableBase, INavigationAware
    {
        #region Fields
        private int personId;
        private readonly Func<PersonTalonsCollectionViewModel> personTalonsViewModelFactory;
        private readonly Func<PersonCommissionsCollectionViewModel> personCommissionsViewModelFactory;
        #endregion

        #region  Constructors
        public PersonTalonsCommissionsHospitalisationsViewModel(Func<PersonTalonsCollectionViewModel> personTalonsViewModelFactory,
                                Func<PersonCommissionsCollectionViewModel> personCommissionsViewModelFactory)
        {           
            if (personTalonsViewModelFactory == null)
            {
                throw new ArgumentNullException("personTalonsViewModelFactory");
            }
            if (personCommissionsViewModelFactory == null)
            {
                throw new ArgumentNullException("personCommissionsViewModelFactory");
            }
            personId = SpecialValues.NonExistingId;
            this.personTalonsViewModelFactory = personTalonsViewModelFactory;
            this.personCommissionsViewModelFactory = personCommissionsViewModelFactory;
        }
        #endregion

        public ICommand EditTalonCommand 
        {
            get 
            {
                if (personTalonsVM == null)
                    PersonTalonsVM = personTalonsViewModelFactory();
                return personTalonsVM.EditTalonCommand; 
            } 
        }

        public ICommand RemoveTalonCommand
        {
            get
            {
                if (personTalonsVM == null)
                    PersonTalonsVM = personTalonsViewModelFactory();
                return personTalonsVM.RemoveTalonCommand;
            }
        }

        public ICommand LinkTalonToHospitalisationCommand
        {
            get
            {
                if (personTalonsVM == null)
                    PersonTalonsVM = personTalonsViewModelFactory();
                return personTalonsVM.LinkTalonToHospitalisationCommand;
            }
        }

        public ICommand RemoveCommissionProtocolCommand
        {
            get
            {
                if (personCommissionsVM == null)
                    PersonCommissionsVM = personCommissionsViewModelFactory();
                return personCommissionsVM.RemoveCommissionProtocolCommand;
            }
        }

        public ICommand PrintCommissionProtocolCommand
        {
            get
            {
                if (personCommissionsVM == null)
                    PersonCommissionsVM = personCommissionsViewModelFactory();
                return personCommissionsVM.PrintCommissionProtocolCommand;
            }
        }     
                        
        private PersonCommissionsCollectionViewModel personCommissionsVM;
        public PersonCommissionsCollectionViewModel PersonCommissionsVM
        {
            get { return personCommissionsVM; }
            set { SetProperty(ref personCommissionsVM, value); }
        }

        private PersonTalonsCollectionViewModel personTalonsVM;
        public PersonTalonsCollectionViewModel PersonTalonsVM
        {
            get { return personTalonsVM; }
            set { SetProperty(ref personTalonsVM, value); }
        }

        public void Initialize(int personId)
        {
            this.personId = personId;

            if (personCommissionsVM == null)
                PersonCommissionsVM = personCommissionsViewModelFactory();
            PersonCommissionsVM.PersonId = personId;

            if (personTalonsVM == null)
                PersonTalonsVM = personTalonsViewModelFactory();
            PersonTalonsVM.PersonId = personId;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var targetPersonId = (int?)navigationContext.Parameters[ParameterNames.PatientId] ?? SpecialValues.NonExistingId;
            if (targetPersonId != personId)
                Initialize(targetPersonId);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            //We use only one view-model for patient info, thus we says that current view-model can accept navigation requests
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //TODO: place here logic for current view being deactivated
        }
    }
}
