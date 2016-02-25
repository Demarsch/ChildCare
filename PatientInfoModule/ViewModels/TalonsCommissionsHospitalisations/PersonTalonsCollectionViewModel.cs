using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Navigation;
using Core.Data;
using Core.Data.Classes;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;
using Prism.Events;
using Prism.Regions;
using System.Windows.Media;
using System.Windows.Input;
using PatientInfoModule.Services;

namespace PatientInfoModule.ViewModels
{
    public class PersonTalonsCollectionViewModel : BindableBase
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly ILog logService;
        private readonly IDialogService messageService;
        private readonly IEventAggregator eventAggregator;
        private readonly IDialogServiceAsync dialogService;
        private readonly Func<CreateTalonViewModel> createTalonViewModelFactory;
        #endregion

        #region  Constructors
        public PersonTalonsCollectionViewModel(ICommissionService commissionService, ILog logService, IDialogService messageService, IEventAggregator eventAggregator, 
            IDialogServiceAsync dialogService, Func<CreateTalonViewModel> createTalonViewModelFactory)
        {
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (createTalonViewModelFactory == null)
            {
                throw new ArgumentNullException("createTalonViewModelFactory");
            }  
            this.eventAggregator = eventAggregator;
            this.commissionService = commissionService;
            this.logService = logService;
            this.messageService = messageService;
            this.dialogService = dialogService;
            this.createTalonViewModelFactory = createTalonViewModelFactory;

            editTalonCommand = new DelegateCommand<int?>(EditTalon);
            removeTalonCommand = new DelegateCommand<int?>(RemoveTalon);
            linkTalonToHospitalisationCommand = new DelegateCommand<int?>(LinkTalonToHospitalisation);

            Talons = new ObservableCollectionEx<PersonTalonViewModel>();
            BusyMediator = new BusyMediator();
        }
        
        #endregion

        #region Properties
        private DelegateCommand<int?> editTalonCommand;
        public ICommand EditTalonCommand { get { return editTalonCommand; } }

        private DelegateCommand<int?> removeTalonCommand;
        public ICommand RemoveTalonCommand { get { return removeTalonCommand; } }

        private DelegateCommand<int?> linkTalonToHospitalisationCommand;
        public ICommand LinkTalonToHospitalisationCommand { get { return linkTalonToHospitalisationCommand; } }

        public BusyMediator BusyMediator { get; set; }

        public ObservableCollectionEx<PersonTalonViewModel> Talons { get; set; }

        private PersonTalonViewModel selectedTalon;
        public PersonTalonViewModel SelectedTalon
        {
            get { return selectedTalon; }
            set 
            {
                if (SetProperty(ref selectedTalon, value) && value != null)
                    eventAggregator.GetEvent<PubSubEvent<PersonTalonViewModel>>().Publish(value);   
            }
        }

        private int personId;
        public int PersonId
        {
            get { return personId; }
            set 
            {
                if (SetProperty(ref personId, value) & !SpecialValues.IsNewOrNonExisting(value))
                    LoadTalonsAsync();            
            }
        }

        #endregion

        #region Methods

        private async void LoadTalonsAsync()
        {
            BusyMediator.Activate("Загрузка талонов пациента...");
            logService.Info("Loading patient talons...");
            Talons.Clear();
            IDisposableQueryable<PersonTalon> talonsQuery = null;
            try
            {
                talonsQuery = commissionService.GetPatientTalons(PersonId);

                var talonsSelectQuery = await talonsQuery.Select(x => new
                    {
                        Id = x.Id,
                        Number = x.TalonNumber,
                        TalonDate = x.TalonDateTime,
                        HospitalisationNumber = "нет И/Б",
                        IsCompleted = x.IsCompleted,
                        HelpType = x.MedicalHelpTypeId.HasValue ? x.MedicalHelpType.Code : string.Empty,
                        CodeMKB = x.MKB,
                        Address = x.PersonAddress.UserText
                    }).ToArrayAsync();

                var result = talonsSelectQuery.Select(x => new PersonTalonViewModel()
                    {
                        Id = x.Id,
                        TalonNumber = " " + x.Number,
                        HospitalisationNumber = " - " + x.HospitalisationNumber + " - ",
                        TalonDate = "(добавлен " + x.TalonDate.ToShortDateString() + ")",
                        MKB = !string.IsNullOrEmpty(x.CodeMKB) ? x.CodeMKB : "отсутствует",
                        MedHelpType = x.HelpType,
                        IsCompleted = x.IsCompleted,
                        TalonState = !x.IsCompleted.HasValue ? " - создан" : (x.IsCompleted == false ? " - в работе" : " - закрыт"),
                        Address = x.Address                        
                    }).ToArray();

                Talons.AddRange(result);
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load patient talons");
                messageService.ShowError("Не удалось загрузить талоны пациента. ");
            }
            finally
            {
                if (talonsQuery != null)
                    talonsQuery.Dispose();
                BusyMediator.Deactivate();
            }
        }

        private async void EditTalon(int? selectedTalonId)
        {
            if (SpecialValues.IsNewOrNonExisting(PersonId))
            {
                messageService.ShowWarning("Не выбран пациент.");
                return;
            }

            var editTalonViewModel = createTalonViewModelFactory();
            if (!selectedTalonId.HasValue || SpecialValues.IsNewOrNonExisting(selectedTalonId.Value))
                editTalonViewModel.Initialize(PersonId);
            else
                editTalonViewModel.Initialize(PersonId, selectedTalonId.Value);
            var result = await dialogService.ShowDialogAsync(editTalonViewModel);
            if (result == true && editTalonViewModel.SaveIsSuccessful)
                LoadTalonsAsync();
        }

        private async void RemoveTalon(int? selectedTalonId)
        {
            if (SpecialValues.IsNewOrNonExisting(PersonId))
            {
                messageService.ShowWarning("Не выбран пациент.");
                return;
            }
            if (!selectedTalonId.HasValue || SpecialValues.IsNewOrNonExisting(selectedTalonId.Value))
            {
                messageService.ShowWarning("Выберите талон");
                return;
            }
            if (messageService.AskUser("Удалить талон ") == true)
            {
                bool isOk = await commissionService.RemoveTalon(selectedTalonId.Value);
                if (isOk)
                    LoadTalonsAsync();
            }
        }

        private void LinkTalonToHospitalisation(int? selectedTalonId)
        {
            if (SpecialValues.IsNewOrNonExisting(PersonId))
            {
                messageService.ShowWarning("Не выбран пациент.");
                return;
            }
            if (!selectedTalonId.HasValue || SpecialValues.IsNewOrNonExisting(selectedTalonId.Value))
            {
                messageService.ShowWarning("Выберите талон");
                return;
            }
            messageService.ShowWarning("Отсутствует функционал по работе с И/Б");
        }

        #endregion      
       
    }
}
