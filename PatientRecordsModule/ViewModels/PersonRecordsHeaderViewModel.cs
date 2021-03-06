﻿using System;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Wpf.Events;
using Core.Wpf.Services;
using log4net;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Shared;
using Core.Wpf.Mvvm;
using System.Threading;
using System.Threading.Tasks;
using Core.Misc;
using System.Linq;
using Shared.PatientRecords.Services;
using System.Data.Entity;
using Core.Extensions;
using Shared.PatientRecords.DTO;
using Microsoft.Practices.Unity;
using Core.Wpf.Misc;
using System.Collections.Generic;

namespace Shared.PatientRecords.ViewModels
{
    public class PersonRecordsHeaderViewModel : BindableBase, IActiveAware
    {
        #region Fields       

        private readonly Func<PersonRecordsToolboxViewModel> personRecordsToolboxViewModelFactory;

        #endregion

        #region Constructors
        public PersonRecordsHeaderViewModel(PersonRecordsToolboxViewModel personRecordsToolboxViewModel, Func<PersonRecordsToolboxViewModel> personRecordsToolboxViewModelFactory)
        {        
            this.personRecordsToolboxViewModelFactory = personRecordsToolboxViewModelFactory;
            PersonRecordsToolboxViewModel = personRecordsToolboxViewModel;           
        }

        #endregion

        private PersonRecordsToolboxViewModel personRecordsToolboxViewModel;
        public PersonRecordsToolboxViewModel PersonRecordsToolboxViewModel
        {
            get { return personRecordsToolboxViewModel; }
            set { SetProperty(ref personRecordsToolboxViewModel, value); }
        }

        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    IsActiveChanged(this, EventArgs.Empty);
                    OnPropertyChanged(() => IsActive);
                    if (value)
                    {
                        ActivateHeader();
                    }
                }
            }
        }

        private void ActivateHeader()
        {
            if (personRecordsToolboxViewModel == null)
                PersonRecordsToolboxViewModel = personRecordsToolboxViewModelFactory();

            PersonRecordsToolboxViewModel.ActivatePersonRecords();
        }        

        #region Events
        public event EventHandler IsActiveChanged = delegate { };
        #endregion

       
    }
}
