using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using DataLib;
using Core;
using log4net;
using GalaSoft.MvvmLight;
using MainLib;
using System.Windows.Navigation;

namespace Commission
{
    [PropertyChanged.ImplementPropertyChanged]
    public class SelectCommissionMembersViewModel : ViewModelBase, IDialogViewModel
    {
        private ICommissionService commissionService;
        private IPersonService personService;
        private IDialogService dialogService;
        private ILog log;

        private DateTime commissionBegin;
        private DateTime commissionEnd;
        public List<PersonStaff> resultPersonStaffs = new List<PersonStaff>();

        public SelectCommissionMembersViewModel(DateTime commissionBegin, DateTime commissionEnd, ICommissionService commissionService, IPersonService personService, IDialogService dialogService, ILog log)
        {
            this.commissionService = commissionService;            
            this.personService = personService;
            this.dialogService = dialogService;
            this.log = log;
            this.commissionBegin = commissionBegin;
            this.commissionEnd = commissionEnd;
            this.CloseCommand = new RelayCommand<object>(x => Close((bool?)x));

            Staffs = new ObservableCollection<Staff>(personService.GetAllStaffs());
        }             
             
        private ObservableCollection<Staff> staffs;
        public ObservableCollection<Staff> Staffs
        {
            get { return staffs; }
            set { Set("Staffs", ref staffs, value); }
        }

        private Staff selectedStaff;
        public Staff SelectedStaff
        {
            get { return selectedStaff; }
            set
            {
                if (!Set("SelectedStaff", ref selectedStaff, value) || value == null)
                    return;
                Persons = new ObservableCollection<CheckedListItem>(personService.GetPersonsByStaffId(value.Id, this.commissionBegin, this.commissionEnd).Select(x => new CheckedListItem(){ Id = x.Id, Name = x.FullName, IsChecked = false}));
            }
        }

        private ObservableCollection<CheckedListItem> persons;
        public ObservableCollection<CheckedListItem> Persons
        {
            get { return persons; }
            set { Set("Persons", ref persons, value); }
        }

        #region Implementation IDialogViewModel

        public string Title
        {
            get { return "Выбор членов комиссии"; }
        }

        public string ConfirmButtonText
        {
            get { return "Выбрать"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }
        
        public RelayCommand<object> CloseCommand { get; set; }

        private void Close(bool? validate)
        {
            if (validate == true)
            {
                foreach (var person in Persons.Where(x => x.IsChecked))
                    resultPersonStaffs.Add(personService.GetPersonStaff(person.Id, selectedStaff.Id, commissionBegin, commissionEnd));

                OnCloseRequested(new ReturnEventArgs<bool>(true));
            }
            else 
                OnCloseRequested(new ReturnEventArgs<bool>(false));
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

        #endregion
    }
}
