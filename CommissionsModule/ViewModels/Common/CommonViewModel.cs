using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace CommissionsModule.ViewModels.Common
{
    public class CommonViewModel : BindableBase, IDialogViewModel
    {
        #region Fields

        private readonly Func<PersonTalonsViewModel> personTalonsViewModelFactory;
        private readonly Func<PersonCommissionsViewModel> personCommissionsViewModelFactory;
        #endregion

        #region  Constructors
        public CommonViewModel(Func<PersonTalonsViewModel> personTalonsViewModelFactory,
                                Func<PersonCommissionsViewModel> personCommissionsViewModelFactory)
        {           
            if (personTalonsViewModelFactory == null)
            {
                throw new ArgumentNullException("personTalonsViewModelFactory");
            }
            if (personCommissionsViewModelFactory == null)
            {
                throw new ArgumentNullException("personCommissionsViewModelFactory");
            }
            this.personTalonsViewModelFactory = personTalonsViewModelFactory;
            this.personCommissionsViewModelFactory = personCommissionsViewModelFactory;
            
            CloseCommand = new DelegateCommand<bool?>(Close);

        }
        #endregion

        private PersonCommissionsViewModel personCommissionsVM;
        public PersonCommissionsViewModel PersonCommissionsVM
        {
            get { return personCommissionsVM; }
            set { SetProperty(ref personCommissionsVM, value); }
        }

        private PersonTalonsViewModel personTalonsVM;
        public PersonTalonsViewModel PersonTalonsVM
        {
            get { return personTalonsVM; }
            set { SetProperty(ref personTalonsVM, value); }
        }

        public void Initialize(int personId)
        {
            PersonCommissionsVM = personCommissionsViewModelFactory();
            PersonCommissionsVM.PersonId = personId;

            PersonTalonsVM = personTalonsViewModelFactory();
            PersonTalonsVM.PersonId = personId;
        }
        
        public string Title
        {
            get { return "Какие-то данные"; }
        }

        public string ConfirmButtonText
        {
            get { return "OK"; }
        }

        public string CancelButtonText
        {
            get { return "Закрыть"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        private void Close(bool? validate)
        {
            OnCloseRequested(new ReturnEventArgs<bool>(true));            
        }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(ReturnEventArgs<bool> e)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
