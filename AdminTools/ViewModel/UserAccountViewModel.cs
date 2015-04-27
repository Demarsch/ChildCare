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

namespace AdminTools.ViewModel
{
    class UserAccountViewModel : ObservableObject
    {
        private readonly ISimpleLocator service;
        
        public UserAccountViewModel(ISimpleLocator service)
        {            
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.EditPersonDataCommand = new RelayCommand<object>(EditPersonData);
            this.SaveUserCommand = new RelayCommand(SaveUser);
            UsersSystemInfoSuggestionProvider = new UsersSystemInfoSuggestionProvider(service.Instance<IUserSystemInfoService>());
            IsSearchSuccessful = false;
            AllowSave = false;
        }

        public UserAccountViewModel(ISimpleLocator service, ILog log, int userId)
            : this(service)
        {
        }

        #region Commands

        private UsersSystemInfoSuggestionProvider usersSystemInfoSuggestionProvider;
        public UsersSystemInfoSuggestionProvider UsersSystemInfoSuggestionProvider
        {
            get { return usersSystemInfoSuggestionProvider; }
            set { Set("UsersSystemInfoSuggestionProvider", ref usersSystemInfoSuggestionProvider, value); }
        }

        private RelayCommand saveUserCommand;
        public RelayCommand SaveUserCommand
        {
            get { return saveUserCommand; }
            set { Set("SaveUserCommand", ref saveUserCommand, value); }
        }        

        private RelayCommand<object> editPersonDataCommand;
        public RelayCommand<object> EditPersonDataCommand
        {
            get { return editPersonDataCommand; }
            set { Set("EditPersonDataCommand", ref editPersonDataCommand, value); }
        }

        #endregion 

        private UserViewModel currentUser;
        public UserViewModel CurrentUser
        {
            get { return currentUser; }
            set { Set("CurrentUser", ref currentUser, value); }
        }
                
        private UserSystemInfo selectedUserAD;
        public UserSystemInfo SelectedUserAD
        {
            get { return selectedUserAD; }
            set {
                    if (!Set("SelectedUserAD", ref selectedUserAD, value))
                            return;
                    IsSearchSuccessful = (value != null); 
                }
        }

        private ObservableCollection<Person> persons;
        public ObservableCollection<Person> Persons
        {
            get { return persons; }
            set { Set("Persons", ref persons, value); }
        }

        private Person selectedPerson;
        public Person SelectedPerson
        {
            get { return selectedPerson; }
            set
            {
                Set("SelectedPerson", ref selectedPerson, value);                  
                AllowSave = (value != null);
            }
        }           

        private bool isSearchSuccessful;
        public bool IsSearchSuccessful
        {
            get { return isSearchSuccessful; }
            set 
            { 
                Set("IsSearchSuccessful", ref isSearchSuccessful, value);
                if (value)
                    Persons = new ObservableCollection<Person>(GetPersonsFromMIS(SelectedUserAD.UserName));
                else                                    
                    AllowSave = false;                
            }
        }

        private bool allowSave;
        public bool AllowSave
        {
            get { return allowSave; }
            set { Set("AllowSave", ref allowSave, value); }
        }                
          
        private ICollection<Person> GetPersonsFromMIS(string searchText)
        {
            //return service.Instance<IPatientService>().GetPatients(personFullName);
            return service.Instance<IPersonService>().GetPersonsByFullName(searchText);
        }

        private void EditPersonData(object parameter)
        {
            if (parameter == null) 
                EditPerson(null);
            else
            {
                SelectedPerson = parameter as Person;
                EditPerson(SelectedPerson.Id);
            }
        }

        private void EditPerson(object personId)
        {
            EditPersonViewModel personViewModel = null;

            if (personId == null)            
                personViewModel = new EditPersonViewModel(this.service.Instance<ILog>(), this.service.Instance<IPersonService>());
            else            
                personViewModel = new EditPersonViewModel(this.service.Instance<ILog>(), this.service.Instance<IPersonService>(), personId.ToInt());

            (new EditPersonView() { DataContext = personViewModel, WindowStartupLocation = WindowStartupLocation.CenterScreen }).ShowDialog();

            //TODO: Change verification way
            if (personViewModel.EditPersonDataViewModel == null || personViewModel.EditPersonDataViewModel.TextMessage != "Данные сохранены") return;

            Persons = new ObservableCollection<Person>(GetPersonsFromMIS(SelectedUserAD.UserName));
            SelectedPerson = Persons.FirstOrDefault(x => x.Id == personViewModel.Id);
        }

        private void SaveUser()
        {            
            if (SelectedPerson == null)
            {
                MessageBox.Show("Не выбрана учетная карта.");
                return;
            }
            User user = this.service.Instance<IUserService>().GetUserByPersonId(SelectedPerson.Id);
            if (user == null)
            {
                user = new User();
                user.PersonId = SelectedPerson.Id;
                user.SID = (SelectedUserAD.SID.HasData() ? SelectedUserAD.SID : string.Empty);
                user.BeginDateTime = DateTime.Now;
                user.EndDateTime = user.BeginDateTime;

                string message = string.Empty;
                if (this.service.Instance<IUserService>().Save(user, out message))
                    MessageBox.Show("Данные сохранены");
                else
                {
                    MessageBox.Show("При сохранении возникла ошибка: " + message);
                    this.service.Instance<ILog>().Error(string.Format("Failed to Save user. " + message));
                }

                CurrentUser = new UserViewModel(user);
            }
            else
            {
                MessageBox.Show("У пользователя " + SelectedPerson.FullName + " уже имеется учетная запись.");
                return;
            }
        }
    }
}
