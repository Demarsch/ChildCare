﻿using System;
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
using AdminTools.DTO;
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
            this.SearchInActiveDirectoryCommand = new RelayCommand(SearchInActiveDirectory);
            this.SynchPersonCommand = new RelayCommand<object>(SynchPerson);
            this.SelectPersonCommand = new RelayCommand<object>(SelectPerson);
            this.CreatePersonCommand = new RelayCommand<object>(CreatePerson);
            IsSearching = false;
            IsSearchSuccessful = false;
            SearchLabel = string.Empty;
        }

        public UserAccountViewModel(ISimpleLocator service, ILog log, int userId)
            : this(service)
        {
        }

        private string searchString = string.Empty;
        public string SearchString
        {
            get { return searchString; }
            set
            {
                value = value.Trim();
                if (!Set("SearchString", ref searchString, value))
                    return;
                SearchUsers(value);
            }
        }

        private void SearchUsers(string searchString)
        {

        }

        private string searchLabel = string.Empty;
        public string SearchLabel
        {
            get { return searchLabel; }
            set { Set("SearchLabel", ref searchLabel, value); }            
        }

        private RelayCommand<object> createPersonCommand;
        public RelayCommand<object> CreatePersonCommand
        {
            get { return createPersonCommand; }
            set { Set("CreatePersonCommand", ref createPersonCommand, value); }
        }

        private RelayCommand<object> synchPersonCommand;
        public RelayCommand<object> SynchPersonCommand
        {
            get { return synchPersonCommand; }
            set { Set("SynchPersonCommand", ref synchPersonCommand, value); }
        }

        private RelayCommand<object> selectPersonCommand;
        public RelayCommand<object> SelectPersonCommand
        {
            get { return selectPersonCommand; }
            set { Set("SelectPersonCommand", ref selectPersonCommand, value); }
        }
        
        private RelayCommand searchInActiveDirectoryCommand;
        public RelayCommand SearchInActiveDirectoryCommand
        {
            get { return searchInActiveDirectoryCommand; }
            set { Set("SearchInActiveDirectoryCommand", ref searchInActiveDirectoryCommand, value); }
        }

        private UserViewModel currentUser;
        public UserViewModel CurrentUser
        {
            get { return currentUser; }
            set { Set("CurrentUser", ref currentUser, value); }
        }

        private ObservableCollection<UserSystemInfoDTO> usersAD;
        public ObservableCollection<UserSystemInfoDTO> UsersAD
        {
            get { return usersAD; }
            set { Set("UsersAD", ref usersAD, value); }
        }
        
        private UserSystemInfoDTO selectedUserAD;
        public UserSystemInfoDTO SelectedUserAD
        {
            get { return selectedUserAD; }
            set { Set("SelectedUserAD", ref selectedUserAD, value); }
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
            set {                    
                    if (!Set("SelectedPerson", ref selectedPerson, value))
                        return;
                    CreatePerson(value.Id);
                }
        }

        private bool isSearching;
        public bool IsSearching
        {
            get { return isSearching; }
            set { Set("IsSearching", ref isSearching, value); }
        }               

        private bool isSearchSuccessful;
        public bool IsSearchSuccessful
        {
            get { return isSearchSuccessful; }
            set { Set("IsSearchSuccessful", ref isSearchSuccessful, value); }
        }

        private bool showPopup;
        public bool ShowPopup
        {
            get { return showPopup; }
            set { Set("ShowPopup", ref showPopup, value); }

        }

        private async void SearchInActiveDirectory()
        {
            if (SearchString.Length < 3)
            {               
                SearchLabel = "Результаты поиска: Слишком короткая строка для поиска.";
                IsSearchSuccessful = false;
                return;
            }            
            var task = Task.Factory.StartNew(SearchInActiveDirectoryAsync);
            await task;
        }

        private void SearchInActiveDirectoryAsync()
        {
            try
            {
                IsSearching = true;
                SearchLabel = "Идет поиск ...";
                UsersAD = new ObservableCollection<UserSystemInfoDTO>(service.Instance<IUserSystemInfoService>().Find(SearchString)
                          .Select(x => new UserSystemInfoDTO() { UserName = x.UserName, SID = x.SID, PrincipalName = x.PrincipalName, Enabled = x.Enabled, Persons = GetPersonsFromMIS(x.UserName), 
                              PhotoSource = "pack://application:,,,/Resources;component/Images/Refresh_16x16.png"}).ToArray());  
            }
            catch (AggregateException ex)
            {
                var innerException = ex.InnerExceptions[0];
                MessageBox.Show("В процессе поиска учетной записи произошла ошибка. Возможно отсутствует доступ к Active Directory. Обратитесь в службу поддержки");
                this.service.Instance<ILog>().Error(string.Format("Failed to find user in Active Directory"), innerException);                
            }
            finally
            {
                IsSearching = false;                
                if (!UsersAD.Any())
                {
                    SearchLabel = "Результаты поиска: По Вашему запросу ничего не найдено.";
                    IsSearchSuccessful = false;
                }
                else
                {
                    SearchLabel = "Результаты поиска";
                    IsSearchSuccessful = true;
                }
            }
        }

        private ICollection<Person> GetPersonsFromMIS(string personFullName)
        {
            //return service.Instance<IPatientService>().GetPatients(personFullName);
            return service.Instance<IPersonService>().GetPersonsByFullName(personFullName);
        }

        private void SynchPerson(object parameter)
        {           
            if (SelectedUserAD == null)
            {
                MessageBox.Show("Не выбрана учетная запись из Active Directory");
                return;
            }
            ShowPopup = false;
            if (parameter == null) return;

            if (parameter is UserSystemInfoDTO)
            {
                var personInfo = parameter as UserSystemInfoDTO;
                Persons = new ObservableCollection<Person>(personInfo.Persons);
                if (!personInfo.Persons.Any())                
                    CreatePerson(null);    
                else
                    ShowPopup = true;
            }            
        }

        private void SelectPerson(object parameter)
        {
            if (parameter == null) return;
            
        }

        private void CreatePerson(object personId)
        {
            EditPersonViewModel personViewModel = null;

            if (personId == null)
                personViewModel = new EditPersonViewModel(this.service.Instance<ILog>(), this.service.Instance<IPersonService>());
            else
            {
                var existingUser = this.service.Instance<IUserService>().GetUserByPersonId(personId.ToInt());
                if (existingUser != null)
                {
                    MessageBox.Show("Выбранная учетная запись уже закреплена за пользователем: " + existingUser.Person.FullName);
                    return;
                }
                personViewModel = new EditPersonViewModel(this.service.Instance<ILog>(), this.service.Instance<IPersonService>(), personId.ToInt());
            }               

            (new EditPersonView() { DataContext = personViewModel }).ShowDialog();
            
            //TODO: change Save verification
            if (personViewModel.EditPersonDataViewModel == null || personViewModel.EditPersonDataViewModel.TextMessage != "Данные сохранены") return;
           
            User user = this.service.Instance<IUserService>().GetUserByPersonId(personViewModel.Id);
            if (user == null)
            {
                user = new User();
                user.PersonId = personViewModel.Id;
                user.SID = SelectedUserAD.SID;
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
            
        }
    }
}
