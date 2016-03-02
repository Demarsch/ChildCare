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
using Shared.Patient.Misc;
using Core.Misc;
using PatientInfoModule.Services;

namespace PatientInfoModule.ViewModels
{
    public class CreateTalonViewModel : BindableBase, IDialogViewModel, IDataErrorInfo
    {
        private readonly ICommissionService commissionService;
        private readonly ILog logService;
        private readonly IDialogService messageService;
        private readonly ICacheService cacheService;
        private readonly IUserService userService;
        private CancellationTokenSource currentSavingToken;

        public CreateTalonViewModel(ICommissionService commissionService,
                                      IDialogServiceAsync dialogService,
                                      IDialogService messageService,
                                      ILog logService,
                                      ICacheService cacheService, 
                                      IUserService userService,
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
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
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
            this.userService = userService;

            AddressSuggestionProvider = addressSuggestionProvider;

            BusyMediator = new BusyMediator();
            CloseCommand = new DelegateCommand<bool?>(Close);
            MedicalHelpTypes = new ObservableCollectionEx<FieldValue>();
        }

        #region Properties

        public BusyMediator BusyMediator { get; set; }
        public IAddressSuggestionProvider AddressSuggestionProvider { get; private set; }

        public int TalonId { get; private set; }

        public int PersonId { get; private set; }

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

        private bool talonSaved;
        public bool TalonSaved
        {
            get { return talonSaved; }
            set 
            {
                if (SetProperty(ref talonSaved, value))
                {
                    if (!SpecialValues.IsNewOrNonExisting(TalonId))
                        InUser = commissionService.GetTalonById(TalonId).First().User.Person.ShortName;
                    else
                        InUser = userService.GetCurrentUser().Person.ShortName;
                }
            }
        }

        private string inUser;
        public string InUser
        {
            get { return inUser; }
            set { SetProperty(ref inUser, value); }
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

        public bool SaveIsSuccessful { get; set; }

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

        internal void Initialize(int personId, int talonId = SpecialValues.NewId)
        {
            TalonId = talonId;
            PersonId = personId;
            saveWasRequested = false;
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
                var talon = commissionService.GetTalonById(TalonId).First();
                TalonNumber = talon.TalonNumber;
                SelectedMedicalHelpTypeId = talon.MedicalHelpTypeId;
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
                
                TalonSaved = true;
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
                if (!SpecialValues.IsNewOrNonExisting(TalonId))
                    onDate = commissionService.GetTalonById(TalonId).First().TalonDateTime;
                var medicalHelpTypesTask = Task.Factory.StartNew((Func<object, IEnumerable<MedicalHelpType>>)commissionService.GetCommissionMedicalHelpTypes, onDate);
                await Task.WhenAny(medicalHelpTypesTask, AddressSuggestionProvider.EnsureDataSourceLoadedAsync());
                var medicalHelpTypesQuery = medicalHelpTypesTask.Result.Select(x => new { x.Id, x.Code, x.ShortName }).ToArray();
                MedicalHelpTypes.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- укажите вид помощи -" });
                MedicalHelpTypes.AddRange(medicalHelpTypesQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Code + (!string.IsNullOrEmpty(x.ShortName) ? (" - " + x.ShortName) : string.Empty) }));
                SelectedMedicalHelpTypeId = SpecialValues.NonExistingId;
                TalonSaved = false;
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

        private async Task SaveTalon()
        {
            BusyMediator.Activate("Сохранение талона...");
            logService.Info("Save talon ...");
            if (currentSavingToken != null)
            {
                currentSavingToken.Cancel();
                currentSavingToken.Dispose();
            }
            currentSavingToken = new CancellationTokenSource();
            var token = currentSavingToken.Token;
            SaveIsSuccessful = false;
            try
            {
                PersonTalon talon = commissionService.GetTalonById(TalonId).FirstOrDefault();
                if (talon == null)
                    talon = new PersonTalon() { TalonDateTime = DateTime.Now };
                talon.PersonId = PersonId;
                talon.TalonNumber = TalonNumber;
                talon.MKB = CodeMKB;
                talon.Comment = Comment;
                talon.RecordContractId = commissionService.GetRecordContractsByOptions(OptionValues.HMHContract, talon.TalonDateTime).First().Id;
                talon.MedicalHelpTypeId = SelectedMedicalHelpTypeId;
                talon.IsCompleted = SpecialValues.IsNewOrNonExisting(talon.Id) ? (IsCompleted == false ? (bool?)null : true) : IsCompleted;
                var talonAddress = new PersonAddress() 
                { 
                    Id = talon.PersonAddressId,
                    PersonId = PersonId,
                    AddressTypeId = commissionService.GetAddressTypeByCategory(AddressTypeCategory.Talon.ToString()).First().Id,
                    OkatoId = Location.Id, 
                    UserText = UserText, 
                    House = House, 
                    Building = building,
                    Apartment = Appartment,
                    BeginDateTime = SpecialValues.MinDate, 
                    EndDateTime = SpecialValues.MaxDate 
                };

                var resultAddress = await commissionService.SaveTalonAddress(talonAddress, token);
                talon.PersonAddressId = resultAddress;

                var talonResult = await commissionService.SaveTalon(talon, token);
                TalonId = talonResult;
                TalonSaved = true;
                SaveIsSuccessful = true;
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to save talon for person with Id = {0}", PersonId);
                SaveIsSuccessful = false;
                messageService.ShowError("Не удалось сохранить талон.");
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private bool HasAllRequiredFields()
        {
            DateTime onDate = DateTime.Now;
            var talon = commissionService.GetTalonById(TalonId).FirstOrDefault();
            if (talon != null)
                onDate = talon.TalonDateTime;
            var HMHContract = commissionService.GetRecordContractsByOptions(OptionValues.HMHContract, onDate).FirstOrDefault();
            if (HMHContract == null)
            {
                messageService.ShowError("Отсутствует договор на оказание ВМП.");
                return false;
            }
            var talonAddressType = commissionService.GetAddressTypeByCategory(AddressTypeCategory.Talon.ToString()).FirstOrDefault();
            if (talonAddressType == null)
            {
                messageService.ShowError("Отсутствует тип адреса по талону.");
                return false;
            }
            return true;
        }

        #endregion

        #region IDialogViewModel

        public string Title
        {
            get { return "Создание талона"; }
        }

        public string ConfirmButtonText
        {
            get { return SpecialValues.IsNewOrNonExisting(TalonId) ? "Создать" : "Сохранить"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        private async void Close(bool? validate)
        {
            if (validate == true)
            {
                if (HasAllRequiredFields() && IsValid)
                {
                    await SaveTalon();
                    OnCloseRequested(new ReturnEventArgs<bool>(true));
                }
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

        #region IDataError

        private bool saveWasRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        private bool IsValid
        {
            get
            {
                saveWasRequested = true;
                OnPropertyChanged(string.Empty);
                return invalidProperties.Count < 1;
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                if (columnName == "TalonNumber")
                {
                    result = string.IsNullOrEmpty(TalonNumber) ? "Укажите номер талона" : string.Empty;
                }
                if (columnName == "CodeMKB")
                {
                    result = string.IsNullOrEmpty(CodeMKB) ? "Укажите корректный код МКБ" : string.Empty;
                }
                if (columnName == "SelectedMedicalHelpTypeId")
                {
                    result = SelectedMedicalHelpTypeId.IsNewOrNonExisting() ? "Укажите вид помощи" : string.Empty;
                }               
                if (columnName == "UserText")
                {
                    result = string.IsNullOrEmpty(UserText) ? "Укажите адрес по документу" : string.Empty;
                }
                if (columnName == "Region")
                {
                    result = Region == null ? "Выберите регион или иностранное государство" : string.Empty;
                }
                if (columnName == "Location")
                {
                    result = Location == null ? "Укажите ОКАТО района, села, города" : string.Empty;
                }
                if (string.IsNullOrEmpty(result))
                {
                    invalidProperties.Remove(columnName);
                }
                else
                {
                    invalidProperties.Add(columnName);
                }
                return result;
            }
        }

        #endregion
    }
}
