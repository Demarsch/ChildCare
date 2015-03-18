﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;

namespace Registry
{
    public class PatientSearchViewModel : ObservableObject
    {
        private System.Windows.Controls.ListBox patientList;

        private const int UserInputSearchThreshold = 3;

        private const int PatientDisplayCount = 5;

        private readonly ILog log;

        private readonly IPatientService patientService;

        public PatientSearchViewModel(IPatientService patientService, ILog log)
        {
            if (patientService == null)
                throw new ArgumentNullException("patientService");
            if (log == null)
                throw new ArgumentNullException("log");
            this.log = log;
            this.patientService = patientService;
            Patients = new ObservableCollection<PersonViewModel>();
            CurrentPatient = new PersonViewModel(null);
            NewPatientCommand = new RelayCommand(NewPatient);
            EditPatientCommand = new RelayCommand(EditPatient);
            CycleThroughPatientListCommand = new RelayCommand<KeyEventArgs>(CycleThroughPatientList);
        }

        private ObservableCollection<PersonViewModel> patients;

        public ObservableCollection<PersonViewModel> Patients
        {
            get { return patients; }
            set { Set("Patients", ref patients, value); }
        }

        private string searchString;

        public string SearchString
        {
            get { return searchString; }
            set
            {
                value = value.Trim();
                if (Set("SearchString", ref searchString, value))
                    SearchPatients(value);
            }
        }

        private void SearchPatients(string searchString)
        {
            IsLookingForPatient = false;
            FailReason = string.Empty;
            NoOneisFound = false;
            Patients.Clear();
            if (searchString == null || searchString.Length < UserInputSearchThreshold)
                return;
            IsLookingForPatient = true;
            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Delay(500)
                .ContinueWith((_, x) => SearchPatientsImpl(x as string), searchString)
                .ContinueWith(PatientsSearched, uiScheduler);
        }

        private IEnumerable<PersonViewModel> SearchPatientsImpl(string searchString)
        {
            if (searchString != SearchString)
                return null;
            return patientService.GetPatients(searchString, PatientDisplayCount).Select(x => new PersonViewModel(x)).ToArray();
        }

        private void PatientsSearched(Task<IEnumerable<PersonViewModel>> sourceTask)
        {
            var anotherSearchWasExecuted = false;
            try
            {
                var result = sourceTask.Result;
                anotherSearchWasExecuted = result == null || sourceTask.AsyncState as string != SearchString;
                if (anotherSearchWasExecuted)
                    return;
                Patients = new ObservableCollection<PersonViewModel>(sourceTask.Result);
            }
            catch (AggregateException ex)
            {
                var innerException = ex.InnerExceptions[0];
                //TODO: probably move this string to separate localizable dll
                FailReason = "В процессе поиск пациента возникла ошибка. Возможно отсутствует связь с базой данной. Повторите поиск еще раз, если ошибка повторится, обратитесь в службу поддержки";
                log.Error(string.Format("Failed to find patients for user input of '{0}'", sourceTask.AsyncState),
                    innerException);
            }
            finally
            {
                if (!anotherSearchWasExecuted)
                {
                    IsLookingForPatient = false;
                    NoOneisFound = patients.Count == 0;
                }
            }
        }

        private void SelectPatient(PersonViewModel person)
        {
            CurrentPatient = person;
        }

        private PersonViewModel selectedPatient;
        //The difference between this property and CurrentPatient is that one is bound to ListBox and may become null when user searches patients.
        //On the other hand CurrentPatient is empty initially (when no patient is selected) and after user selects or creates a patient, it will never become empty again
        //meaning that user can't deselect patient
        public PersonViewModel SelectedPatient
        {
            get { return selectedPatient; }
            set
            {
                Set("SelectedPatient", ref selectedPatient, value);
                if (value == null)
                    return;
                SelectPatient(value);
            }
        }

        private PersonViewModel currentPatient;

        public PersonViewModel CurrentPatient
        {
            get { return currentPatient; }
            set
            {
                var isPatientSelected = IsPatientSelected;
                if (Set("CurrentPatient", ref currentPatient, value) && IsPatientSelected != isPatientSelected)
                    RaisePropertyChanged("IsPatientSelected");
            }
        }

        public bool IsPatientSelected { get { return currentPatient != null; } }

        private bool isLookingForPatient;

        public bool IsLookingForPatient
        {
            get { return isLookingForPatient; }
            set { Set("IsLookingForPatient", ref isLookingForPatient, value); }
        }

        private bool noOneisFound;

        public bool NoOneisFound
        {
            get { return noOneisFound; }
            set { Set("NoOneisFound", ref noOneisFound, value); }
        }

        private string failReason;
        //TODO: use this on view side
        public string FailReason
        {
            get { return failReason; }
            set
            {
                var isFailed = IsFailed;
                if (Set("FailReason", ref failReason, value) && isFailed != IsFailed)
                    RaisePropertyChanged("IsFailed");
            }
        }

        public bool IsFailed { get { return !string.IsNullOrEmpty(failReason); } }

        public ICommand NewPatientCommand { get; private set; }

        private void NewPatient()
        {
            MessageBox.Show("Окно для создания нового пациента");
        }

        public ICommand EditPatientCommand { get; private set; }

        private void EditPatient()
        {
            if (currentPatient.IsEmpty)
                return;
            MessageBox.Show(string.Format("Пациент {0} будет отредактирован именно в этом окне", currentPatient));
        }

        public ICommand CycleThroughPatientListCommand { get; private set; }

        private void CycleThroughPatientList(KeyEventArgs e)
        {
            //if (e.Key == Key.Escape)
            //    patientList.Focus();
            //if (!patientList.HasItems)
            //    return;
            //if (e.Key == Key.Down)
            //    SelectNextItem();
            //else if (e.Key == Key.Up)
            //    SelectPreviousItem();
            //else if (e.Key == Key.Enter)
            //    patientList.Focus();
        }

        private void SelectNextItem()
        {
            var source = patientList.ItemsSource as IList;
            if (source == null)
                return;
            if (patientList.SelectedIndex < source.Count - 1)
                patientList.SelectedIndex += 1;
            else
                patientList.SelectedIndex = 0;
        }

        private void SelectPreviousItem()
        {
            var source = patientList.ItemsSource as IList;
            if (source == null)
                return;
            if (patientList.SelectedIndex == -1)
                patientList.SelectedIndex = 0;
            else if (patientList.SelectedIndex == 0)
                patientList.SelectedIndex = source.Count - 1;
            else
                patientList.SelectedIndex -= 1;
        }
    }
}
