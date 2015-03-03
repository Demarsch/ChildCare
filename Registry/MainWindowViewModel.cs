using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;

namespace Registry
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly ILog log;

        private readonly MainService service;

        private ObservableCollection<Person> people;

        public ObservableCollection<Person> People
        {
            get { return people; }
            set { Set("People", ref people, value); }
        }

        private string userInput;

        public string UserInput
        {
            get { return userInput; }
            set
            {
                value = value.Trim();
                if (Set("UserInput", ref userInput, value))
                    //TODO: run new search asynchrounously
                    SearchPeople();
            }
        }

        public MainWindowViewModel(ILog log, MainService service)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.log = log;
            People = new ObservableCollection<Person>();
            SearchPeopleCommand = new RelayCommand(SearchPeople); 
        }

        private void SearchPeople()
        {
            //TODO: make the constant a DB parameter
            if (userInput != null && userInput.Length > 2)
            {
                var newPeople = service.GetPeople(UserInput);
                People = new ObservableCollection<Person>(newPeople);
            }
            else
                People.Clear();
        }
        //TODO: we probably don't need an explicit command to run the search (implicit is enough when user types something)
        public ICommand SearchPeopleCommand { get; private set; }
    }
}
