﻿using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Registry
{
    class EditPersonViewModel : ObservableObject
    {
        private readonly ILog log;

        private readonly IPersonService service;

        private EditPersonDataViewModel editPersonDataViewModel;
        public EditPersonDataViewModel EditPersonDataViewModel
        {
            get { return editPersonDataViewModel; }
        }

        private EditPersonDataViewModel editPersonRelativeDataViewModel;
        public EditPersonDataViewModel EditPersonRelativeDataViewModel
        {
            get { return editPersonRelativeDataViewModel; }
            set
            {
                Set("EditPersonRelativeDataViewModel", ref editPersonRelativeDataViewModel, value);
            }
        }

        /// <summary>
        /// Use this for creating new person
        /// </summary>
        public EditPersonViewModel(ILog log, IPersonService service)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.log = log;
            IsPersonEditing = true;
            ReturnToPersonEditingCommand = new RelayCommand(ReturnToPersonEditing);
            EditPersonRelativeDataViewModel = new EditPersonDataViewModel(log, service);
        }

        public EditPersonViewModel(ILog log, IPersonService service, int personId)
            : this(log, service)
        {
            Id = personId;
            this.log = log;
        }

        /// <summary>
        /// TODO: Use this for creating new person with default data from search
        /// </summary>
        public EditPersonViewModel(ILog log, IPersonService service, string personData)
            : this(log, service)
        {

        }

        private int id;
        public int Id
        {
            get { return id; }
            set
            {
                if (id == value)
                    return;
                id = value;
                editPersonDataViewModel = new EditPersonDataViewModel(log, service, id);
                SetRelatives();
            }
        }

        public ICommand ReturnToPersonEditingCommand { get; private set; }

        private void ReturnToPersonEditing()
        {
            SelectedRelative = null;
        }

        private PersonRelative selectedRelative;
        public PersonRelative SelectedRelative
        {
            get { return selectedRelative; }
            set
            {
                Set("SelectedRelative", ref selectedRelative, value);
                IsPersonEditing = (selectedRelative == null);
                if (selectedRelative != null)
                    EditPersonRelativeDataViewModel.Id = selectedRelative.RelativeId;
            }
        }

        private bool isPersonEditing;
        public bool IsPersonEditing
        {
            get { return isPersonEditing; }
            set { Set("IsPersonEditing", ref isPersonEditing, value); }
        }


        private async void SetRelatives()
        {
            var task = Task.Factory.StartNew(SetRelativesAsync);
            await task;
        }

        private void SetRelativesAsync()
        {
            var listRelatives = service.GetPersonRelatives(Id);
            //listRelatives.Add(new PersonRelativeDTO()
            //    {
            //        RelativePersonId = -1,
            //        ShortName = "Новый родственник",
            //        RelativeRelationName = string.Empty,
            //        IsRepresentative = false,
            //        PhotoUri = string.Empty
            //    });
            listRelatives.Add(new PersonRelative());
            Relatives = new ObservableCollection<PersonRelative>(listRelatives);
        }

        private ObservableCollection<PersonRelative> relatives;
        public ObservableCollection<PersonRelative> Relatives
        {
            get { return relatives; }
            set { Set("Relatives", ref relatives, value); }
        }
    }
}
