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
using Core.Services;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;
using CommissionsModule.Services;
using Shared.Patient.Misc;
using Core.Misc;

namespace CommissionsModule.ViewModels.Common
{
    public class CreateTalonViewModel : BindableBase, IDialogViewModel, IActiveDataErrorInfo
    {
        private readonly ICommissionService commissionService;
        private readonly ILog logService;
        private readonly IDialogService messageService;
        private readonly ICacheService cacheService;
        private readonly ValidationMediator validator;
        public int talonId;

        public CreateTalonViewModel(ICommissionService commissionService,
                                      IDialogServiceAsync dialogService,
                                      IDialogService messageService,
                                      ILog logService,
                                      ICacheService cacheService, 
                                      IAddressSuggestionProvider addressSuggestionProvider)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }            
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (addressSuggestionProvider == null)
            {
                throw new ArgumentNullException("addressSuggestionProvider");
            }
            this.logService = logService;
            this.commissionService = commissionService;
            this.messageService = messageService;
            this.cacheService = cacheService;

            AddressSuggestionProvider = addressSuggestionProvider;

            BusyMediator = new BusyMediator();
            CloseCommand = new DelegateCommand<bool?>(Close);
            MedicalHelpTypes = new ObservableCollectionEx<FieldValue>();
        }

        #region Properties

        public BusyMediator BusyMediator { get; set; }
        public IAddressSuggestionProvider AddressSuggestionProvider { get; private set; }

        private Okato region;
        public Okato Region
        {
            get { return region; }
            set
            {
                if (SetProperty(ref region, value))
                {
                    AddressSuggestionProvider.SelectedRegion = value;
                    Location = null;
                    UpdateUserText();
                };
            }
        }

        private Okato location;
        public Okato Location
        {
            get { return location; }
            set
            {
                if (SetProperty(ref location, value))
                    UpdateUserText();
            }
        }       

        private string userText;
        public string UserText
        {
            get { return userText; }
            set { SetProperty(ref userText, value); }
        }

        private string house;
        public string House
        {
            get { return house; }
            set { SetProperty(ref house, value); }
        }

        private string building;
        public string Building
        {
            get { return building; }
            set { SetProperty(ref building, value); }
        }

        private string appartment;
        public string Appartment
        {
            get { return appartment; }
            set { SetProperty(ref appartment, value); }
        }

        private string talonNumber;
        public string TalonNumber
        {
            get { return talonNumber; }
            set { SetProperty(ref talonNumber, value); }
        }

        private string codeMKB;
        public string CodeMKB
        {
            get { return codeMKB; }
            set { SetProperty(ref codeMKB, value); }
        }

        private string comment;
        public string Comment
        {
            get { return comment; }
            set { SetProperty(ref comment, value); }
        }

        private bool isCompleted;
        public bool IsCompleted
        {
            get { return isCompleted; }
            set { SetProperty(ref isCompleted, value); }
        }

        private ObservableCollectionEx<FieldValue> medicalHelpTypes;
        public ObservableCollectionEx<FieldValue> MedicalHelpTypes
        {
            get { return medicalHelpTypes; }
            set { SetProperty(ref medicalHelpTypes, value); }
        }

        private int selectedMedicalHelpTypeId;
        public int SelectedMedicalHelpTypeId
        {
            get { return selectedMedicalHelpTypeId; }
            set { SetProperty(ref selectedMedicalHelpTypeId, value); }
            
        }

        #endregion

        #region Methods

        private void UpdateUserText()
        {
            UserText = location == null
                ? region == null
                    ? string.Empty
                    : region.FullName
                : location.FullName;
        }

        internal void Initialize(int talonId = 0)
        {
            this.talonId = talonId;
            LoadDataSources();
            if (!SpecialValues.IsNewOrNonExisting(talonId))
                LoadTalonDataAsync();
        }

        private void LoadTalonDataAsync()
        {
            BusyMediator.Activate("Загрузка данных...");
            logService.Info("Loading data sources for talons...");
            try
            {
                var talon = commissionService.GetTalonById(this.talonId).First();
                TalonNumber = talon.TalonNumber;
                SelectedMedicalHelpTypeId = talon.MedicalHelpTypeId.HasValue ? talon.MedicalHelpTypeId.Value : SpecialValues.NonExistingId;
                CodeMKB = talon.MKB;
                Comment = talon.Comment;
                IsCompleted = (talon.IsCompleted == true) ? true : false;
                Appartment = talon.PersonAddress.Apartment;
                Building = talon.PersonAddress.Building;
                House = talon.PersonAddress.House;
                var okato = cacheService.GetItemById<Okato>(talon.PersonAddress.OkatoId);
                if (okato == null)
                {
                    Region = null;
                    Location = null;                    
                }
                else if (okato.IsRegion || okato.IsForeignCountry)
                {
                    Region = okato;
                    Location = null;                    
                }
                else
                {
                    var regionCode = okato.CodeOKATO.Substring(0, 2);
                    Region = cacheService.GetItems<Okato>().First(x => x.CodeOKATO.StartsWith(regionCode) && x.IsRegion && okato.FullName.StartsWith(x.FullName));
                    Location = okato;
                }
                UserText = talon.PersonAddress.UserText;
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources for talons");
                messageService.ShowError("Не удалось загрузить данные по талону. ");
            }
            finally
            {              
                BusyMediator.Deactivate();
            }
        }

        private async void LoadDataSources()
        {
            MedicalHelpTypes.Clear();
            BusyMediator.Activate("Загрузка данных...");
            logService.Info("Loading data sources for talons...");
            try
            {
                DateTime onDate = DateTime.Now;
                if (!SpecialValues.IsNewOrNonExisting(talonId))
                    onDate = commissionService.GetTalonById(talonId).First().TalonDateTime;
                var medicalHelpTypesTask = Task.Factory.StartNew((Func<object, IEnumerable<MedicalHelpType>>)commissionService.GetCommissionMedicalHelpTypes, onDate);
                await Task.WhenAny(medicalHelpTypesTask, AddressSuggestionProvider.EnsureDataSourceLoadedAsync());
                var medicalHelpTypesQuery = medicalHelpTypesTask.Result.Select(x => new { x.Id, x.Code, x.ShortName }).ToArray();
                MedicalHelpTypes.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- укажите вид помощи -" });
                MedicalHelpTypes.AddRange(medicalHelpTypesQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Code + (!string.IsNullOrEmpty(x.ShortName) ? (" - " + x.ShortName) : string.Empty) }));
                SelectedMedicalHelpTypeId = SpecialValues.NonExistingId;
                
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources for talons");
                messageService.ShowError("Не удалось загрузить данные по талону. ");
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private bool IsTalonValid()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDialogViewModel

        public string Title
        {
            get { return "Создание талона"; }
        }

        public string ConfirmButtonText
        {
            get { return !SpecialValues.IsNewOrNonExisting(talonId) ? "Создать" : "Сохранить"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        private void Close(bool? validate)
        {
            if (validate == true)
            {
                if(IsTalonValid())
                    OnCloseRequested(new ReturnEventArgs<bool>(true));
            }
            else
                OnCloseRequested(new ReturnEventArgs<bool>(false));
        }        

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(ReturnEventArgs<bool> e)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region Errors
        #region IDataErrorInfo validation

        public string this[string columnName]
        {
            get { return validator[columnName]; }
        }

        public string Error
        {
            get { return validator.Error; }
        }

        public bool Validate()
        {
            return validator.Validate();
        }

        public void CancelValidation()
        {
            validator.CancelValidation();
        }

        private class ValidationMediator : ValidationMediator<CreateTalonViewModel>
        {
            public ValidationMediator(CreateTalonViewModel associatedItem)
                : base(associatedItem)
            {
            }

            protected override void OnValidateProperty(string propertyName)
            {
                if (PropertyNameEquals(propertyName, x => x.Region))
                {
                    ValidateRegion();
                }
                else if (PropertyNameEquals(propertyName, x => x.UserText))
                {
                    ValidateUserText();
                }
                else if (PropertyNameEquals(propertyName, x => x.House))
                {
                    ValidateHouse();
                }
            }

            protected override void RaiseAssociatedObjectPropertyChanged()
            {
                AssociatedItem.OnPropertyChanged(string.Empty);
            }

            protected override void OnValidate()
            {
                ValidateRegion();
                ValidateUserText();
                ValidateHouse();
            }

            private void ValidateHouse()
            {
                SetError(x => x.House, AssociatedItem.Region == null ? "Укажите номер дома" : string.Empty);
            }

            private void ValidateUserText()
            {
                SetError(x => x.UserText, AssociatedItem.Region == null ? "Укажите адрес по документу" : string.Empty);
            }

            private void ValidateRegion()
            {
                SetError(x => x.Region, AssociatedItem.Region == null ? "Выберите регион или иностранное государство" : string.Empty);
            }           
        }

        #endregion
        #endregion
    }
}
