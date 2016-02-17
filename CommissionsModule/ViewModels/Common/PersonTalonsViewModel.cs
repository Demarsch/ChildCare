using CommissionsModule.Services;
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

namespace CommissionsModule.ViewModels.Common
{
    public class PersonTalonsViewModel : BindableBase
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
        public PersonTalonsViewModel(ICommissionService commissionService, ILog logService, IDialogService messageService, IEventAggregator eventAggregator, 
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

            addTalonCommand = new DelegateCommand(AddTalon);
            editTalonCommand = new DelegateCommand(EditTalon);

            Talons = new ObservableCollectionEx<PersonTalonViewModel>();
            BusyMediator = new BusyMediator();
        }
        #endregion

        #region Properties
        private DelegateCommand addTalonCommand;
        public ICommand AddTalonCommand { get { return addTalonCommand; } }

        private DelegateCommand editTalonCommand;
        public ICommand EditTalonCommand { get { return editTalonCommand; } }

        public BusyMediator BusyMediator { get; set; }

        public ObservableCollectionEx<PersonTalonViewModel> Talons { get; set; }

        private PersonTalonViewModel selectedTalon;
        public PersonTalonViewModel SelectedTalon
        {
            get { return selectedTalon; }
            set 
            {
                SetProperty(ref selectedTalon, value);
                if (SetProperty(ref selectedTalon, value) && value != null)
                    eventAggregator.GetEvent<PubSubEvent<int>>().Publish(value.Id);   
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
                        HospitalisatoinNumber = "нет И/Б",
                        IsCompleted = x.IsCompleted,
                        HelpType = x.MedicalHelpTypeId.HasValue ? x.MedicalHelpType.Code : string.Empty,
                        CodeMKB = x.MKB,
                        Address = x.PersonAddress.UserText
                    }).ToArrayAsync();

                var result = talonsSelectQuery.Select(x => new PersonTalonViewModel()
                    {
                        Id = x.Id,
                        TalonNumber = x.Number,
                        HospitalisationNumber = " - " + x.HospitalisatoinNumber + " - ",
                        TalonDate = "(добавлен " + x.TalonDate.ToShortDateString() + ")",
                        MKB = !string.IsNullOrEmpty(x.CodeMKB) ? "МКБ: " + x.CodeMKB : string.Empty,
                        MedHelpType = x.HelpType,
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

        private async void AddTalon()
        {
            var createTalonViewModel = createTalonViewModelFactory();
            createTalonViewModel.Initialize();
            var result = await dialogService.ShowDialogAsync(createTalonViewModel);
            if (result == true)
            {

            }
        }

        private async void EditTalon()
        {
            var createTalonViewModel = createTalonViewModelFactory();
            createTalonViewModel.Initialize(SelectedTalon.Id);
            var result = await dialogService.ShowDialogAsync(createTalonViewModel);
            if (result == true)
            {

            }
        }

        #endregion      
       
    }
}
