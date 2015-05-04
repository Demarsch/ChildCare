﻿using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MainLib
{
    public class EditPersonViewModel : ObservableObject
    {
        private readonly ILog log;

        private readonly IPersonService service;

        private readonly IDialogService dialogService;

        private EditPersonDataViewModel editPersonDataViewModel;
        public EditPersonDataViewModel EditPersonDataViewModel
        {
            get { return editPersonDataViewModel; }
            set
            {
                Set("EditPersonDataViewModel", ref editPersonDataViewModel, value);
            }
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
        public EditPersonViewModel(ILog log, IPersonService service, IDialogService dialogService)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            if (service == null)
                throw new ArgumentNullException("service");
            if (dialogService == null)
                throw new ArgumentNullException("dialogService");
            this.dialogService = dialogService;
            this.service = service;
            this.log = log;
            IsPersonEditing = true;
            ReturnToPersonEditingCommand = new RelayCommand(ReturnToPersonEditing);
            EditPersonRelativeDataViewModel = new EditPersonDataViewModel(log, service, dialogService);
            EditPersonDataViewModel = new EditPersonDataViewModel(log, service, dialogService);
        }

        public EditPersonViewModel(ILog log, IPersonService service, IDialogService dialogService, int personId)
            : this(log, service, dialogService)
        {
            Id = personId;
            this.log = log;
        }

        /// <summary>
        /// TODO: Use this for creating new person with default data from search
        /// </summary>
        public EditPersonViewModel(ILog log, IPersonService service, IDialogService dialogService, string personData)
            : this(log, service, dialogService)
        {

        }

        private int id;
        public int Id
        {
            get { return id; }
            set
            {
                Set("Id", ref id, value);
                editPersonDataViewModel.Id = id;
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
