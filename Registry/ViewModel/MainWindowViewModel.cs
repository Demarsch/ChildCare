using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;

namespace Registry
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly ILog log;

        private readonly MainService service;

        private ObservableCollection<PersonViewModel> patients;

        public ObservableCollection<PersonViewModel> Patients
        {
            get { return patients; }
            set { Set("Patients", ref patients, value); }
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
            Patients = new ObservableCollection<PersonViewModel>();
            SearchPatientsCommand = new RelayCommand(SearchPeople);
            SelectPatientCommand = new RelayCommand<PersonViewModel>(SelectPatient);
        }

        //TODO: we probably don't need an explicit command to run the search (implicit is enough when user types something)
        public ICommand SearchPatientsCommand { get; private set; }

        private void SearchPeople()
        {
            Patients.Clear();
            //TODO: make the constant a DB parameter
            if (userInput != null && userInput.Length > 2)
            {
                var newPeople = service.GetPeople(UserInput);
                Patients = new ObservableCollection<PersonViewModel>(newPeople.Select(x => new PersonViewModel(x, this)));
            }
        }

        public ICommand SelectPatientCommand { get; private set; }

        private void SelectPatient(PersonViewModel person)
        {
            SelectedPatient = person;
            MessageBox.Show(person == null
                ? "На самом деле мы никого не выбрали"
                : string.Format("Выбран пациент {0}. Его информация должна отобразиться в топлевелпэйшнтинфо", person.FullName));
        }

        private PersonViewModel selectedPatient;

        public PersonViewModel SelectedPatient
        {
            get { return selectedPatient; }
            set { Set("SelectedPatient", ref selectedPatient, value); }
        }
    }
}
