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
using AdminTools.DTO;

namespace AdminTools.ViewModel
{
    class UserAccountViewModel : ObservableObject
    {
        private IUserService userService;
        private IUserSystemInfoService userSystemInfoService;
        private ILog log;
        private IPersonService personService;
        private IDialogService dialogService;

        public UserAccountViewModel(IUserService userService, IUserSystemInfoService userSystemInfoService, ILog log, IPersonService personService, IDialogService dialogService)
        {
            this.userService = userService; 
            this.userSystemInfoService = userSystemInfoService;
            this.log = log;
            this.personService = personService;
            this.dialogService = dialogService;

            this.EditPersonDataCommand = new RelayCommand<object>(EditPersonData);
            this.SynchWithActiveDirectoryCommand = new RelayCommand<object>(SynchWithActiveDirectory);
            this.SelectUserADCommand = new RelayCommand<object>(SelectUserAD);
            this.SaveUserCommand = new RelayCommand(SaveUser);
            PersonSuggestionProvider = new PersonSuggestionProvider(personService);
            AllowSave = false;
        }

        #region Commands

        private PersonSuggestionProvider personSuggestionProvider;
        public PersonSuggestionProvider PersonSuggestionProvider
        {
            get { return personSuggestionProvider; }
            set { Set("PersonSuggestionProvider", ref personSuggestionProvider, value); }
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

        private RelayCommand<object> synchWithActiveDirectoryCommand;
        public RelayCommand<object> SynchWithActiveDirectoryCommand
        {
            get { return synchWithActiveDirectoryCommand; }
            set { Set("SynchWithActiveDirectoryCommand", ref synchWithActiveDirectoryCommand, value); }
        }

        private RelayCommand<object> selectUserADCommand;
        public RelayCommand<object> SelectUserADCommand
        {
            get { return selectUserADCommand; }
            set { Set("SelectUserADCommand", ref selectUserADCommand, value); }
        }
        #endregion

        private UserViewModel currentUser;
        public UserViewModel CurrentUser
        {
            get { return currentUser; }
            set { Set("CurrentUser", ref currentUser, value); }
        }

        private ObservableCollection<UserSystemInfo> usersAD;
        public ObservableCollection<UserSystemInfo> UsersAD
        {
            get { return usersAD; }
            set { Set("UsersAD", ref usersAD, value); }
        }

        private UserSystemInfo selectedUserAD;
        public UserSystemInfo SelectedUserAD
        {
            get { return selectedUserAD; }
            set { Set("SelectedUserAD", ref selectedUserAD, value); }
        }

        private ObservableCollection<PersonDTO> persons;
        public ObservableCollection<PersonDTO> Persons
        {
            get { return persons; }
            set { Set("Persons", ref persons, value); }
        }

        private PersonDTO selectedPerson;
        public PersonDTO SelectedPerson
        {
            get { return selectedPerson; }
            set
            {
                if (!Set("SelectedPerson", ref selectedPerson, value))
                    return;
                AllowSave = (value != null);
            }
        }

        private bool allowSave;
        public bool AllowSave
        {
            get { return allowSave; }
            set
            {
                Set("AllowSave", ref allowSave, value);
                if (value)
                {
                    Persons = new ObservableCollection<PersonDTO>(new List<PersonDTO>() { SelectedPerson });
                    SelectedPerson = Persons.First();
                }
            }
        }

        private bool showPopup;
        public bool ShowPopup
        {
            get { return showPopup; }
            set { Set("ShowPopup", ref showPopup, value); }
        }   

        private void SynchWithActiveDirectory(object parameter)
        {
            if (parameter == null)
                return;
            
            /*var data = new List<UserSystemInfo>();
            data.Add(new UserSystemInfo() { UserName = "Жариков Игорь Александрович", PrincipalName = "izharikov@lpu", Enabled = true, SID = "izharikov" });
            data.Add(new UserSystemInfo() { UserName = "Жариков Петр Петрович", PrincipalName = "pzharikov@lpu", Enabled = false, SID = "pzharikov" });

            UsersAD = new ObservableCollection<UserSystemInfo>(data);*/

            UsersAD = new ObservableCollection<UserSystemInfo>(userSystemInfoService.Find(SelectedPerson.FullName));
            if (UsersAD.Any())
                ShowPopup = true;
            else
                MessageBox.Show("В Active Directory не найдено ни одного совпадения по Вашему запросу.");
        }

        private void SelectUserAD(object parameter)
        {
            if (parameter == null)
                return;
            SelectedUserAD = parameter as UserSystemInfo; 
            ShowPopup = false;
            SelectedPerson.SID = SelectedUserAD.SID;
            SelectedPerson.PrincipalName = SelectedUserAD.PrincipalName;
            SelectedPerson.Enabled = SelectedUserAD.Enabled;
        }

        private void EditPersonData(object parameter)
        {
            if (parameter == null) 
                EditPerson(null);
            else
            {
                SelectedPerson = parameter as PersonDTO;
                EditPerson(SelectedPerson.Id);
            }
        }

        private void EditPerson(object personId)
        {
            EditPersonViewModel personViewModel = null;

            if (personId == null)            
                personViewModel = new EditPersonViewModel(log, personService, dialogService);
            else
                personViewModel = new EditPersonViewModel(log, personService, dialogService, personId.ToInt());

            (new EditPersonView() { DataContext = personViewModel, WindowStartupLocation = WindowStartupLocation.CenterScreen }).ShowDialog();

            //TODO: Change verification way
            if (personViewModel.EditPersonDataViewModel == null || personViewModel.EditPersonDataViewModel.TextMessage != "Данные сохранены") return;

            SelectedPerson.FullName = personViewModel.EditPersonDataViewModel.LastName + " " + personViewModel.EditPersonDataViewModel.FirstName + " " + personViewModel.EditPersonDataViewModel.MiddleName;
            SelectedPerson.BirthDate = personViewModel.EditPersonDataViewModel.BirthDate;
            SelectedPerson.Snils = personViewModel.EditPersonDataViewModel.SNILS;
        }

        private void SaveUser()
        {            
            if (SelectedPerson == null)
            {
                MessageBox.Show("Не выбрана учетная карта.");
                return;
            }
            if (SelectedPerson.SID == string.Empty && MessageBox.Show("У пользователя отсутствует SID. Продолжить ?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            User user = userService.GetUserByPersonId(SelectedPerson.Id);
            if (user == null)
            {
                user = new User();
                user.Person = personService.GetPersonById(SelectedPerson.Id);
                user.SID = (SelectedUserAD != null && SelectedUserAD.SID.HasData() ? SelectedUserAD.SID : string.Empty);
                user.BeginDateTime = DateTime.Now;
                user.EndDateTime = (DateTime?)null;

                string message = string.Empty;
                if (userService.Save(user, out message))
                    MessageBox.Show("Данные сохранены");
                else
                {
                    MessageBox.Show("При сохранении возникла ошибка: " + message);
                    log.Error(string.Format("Failed to Save user. " + message));
                }

                CurrentUser = new UserViewModel(user, personService);
            }
            else
            {
                MessageBox.Show("У пользователя " + SelectedPerson.FullName + " уже имеется учетная запись.");
                return;
            }
        }
    }
}
