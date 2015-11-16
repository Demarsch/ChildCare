using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using Core.Misc;
using Core.Services;
using Core.Wpf.Misc;
using PatientInfoModule.Misc;
using Prism.Commands;

namespace PatientInfoModule.ViewModels
{
    public class AddressViewModel : TrackableBindableBase, IDisposable, IChangeTrackerMediator, IActiveDataErrorInfo
    {
        private readonly ICacheService cacheService;

        private readonly ValidationMediator validator;

        public AddressViewModel(ICacheService cacheService, IAddressSuggestionProvider addressSuggestionProvider)
        {
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (addressSuggestionProvider == null)
            {
                throw new ArgumentNullException("addressSuggestionProvider");
            }
            this.cacheService = cacheService;
            AddressSuggestionProvider = addressSuggestionProvider;
            validator = new ValidationMediator(this);
            AddressTypes = cacheService.GetItems<AddressType>();
            DeleteCommand = new DelegateCommand(Delete);
            ChangeTracker = new ChangeTrackerEx<AddressViewModel>(this);
        }

        public IAddressSuggestionProvider AddressSuggestionProvider { get; private set; }   

        private int? addressTypeId;

        public int? AddressTypeId
        {
            get { return addressTypeId; }
            set
            {
                if (SetTrackedProperty(ref addressTypeId, value))
                {
                    OnPropertyChanged(() => StringRepresentation);
                }
            }
        }

        private Okato region;

        public Okato Region
        {
            get { return region; }
            set
            {
                if (SetTrackedProperty(ref region, value))
                {
                    OnPropertyChanged(() => StringRepresentation);
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
                if (SetTrackedProperty(ref location, value))
                {
                    OnPropertyChanged(() => StringRepresentation);
                    UpdateUserText();
                }
            }
        }

        private void UpdateUserText()
        {
            UserText = location == null
                ? region == null
                    ? string.Empty
                    : region.FullName
                : location.FullName;
        }

        private string userText;

        public string UserText
        {
            get { return userText; }
            set { SetTrackedProperty(ref userText, value); }
        }

        private string house;

        public string House
        {
            get { return house; }
            set { SetTrackedProperty(ref house, value); }
        }

        private string building;

        public string Building
        {
            get { return building; }
            set { SetTrackedProperty(ref building, value); }
        }

        private string appartment;

        public string Appartment
        {
            get { return appartment; }
            set { SetTrackedProperty(ref appartment, value); }
        }

        private DateTime? fromDate;

        public DateTime? FromDate
        {
            get { return fromDate; }
            set
            {
                if (value.HasValue)
                {
                    value = value.Value.Date;
                }
                if (SetTrackedProperty(ref fromDate, value))
                {
                    OnPropertyChanged(() => IsActive);
                    if (FromDate.GetValueOrDefault(SpecialValues.MinDate) > ToDate.GetValueOrDefault(SpecialValues.MaxDate))
                    {
                        ToDate = FromDate;
                    }
                    OnPropertyChanged(() => StringRepresentation);
                }
            }
        }

        private DateTime? toDate;

        public DateTime? ToDate
        {
            get { return toDate; }
            set
            {
                if (value.HasValue)
                {
                    value = value.Value.Date;
                }
                if (SetTrackedProperty(ref toDate, value))
                {
                    HasToDate = value.HasValue;
                    OnPropertyChanged(() => IsActive);
                    if (FromDate.GetValueOrDefault(SpecialValues.MinDate) > ToDate.GetValueOrDefault(SpecialValues.MaxDate))
                    {
                        FromDate = ToDate;
                    }
                    OnPropertyChanged(() => StringRepresentation);
                }
            }
        }

        private bool hasToDate;

        public bool HasToDate
        {
            get { return hasToDate; }
            set
            {
                if (SetProperty(ref hasToDate, value) && !value)
                {
                    ToDate = null;
                }
            }
        }

        private int id;

        private int personId;

        public PersonAddress Model
        {
            get
            {
                return new PersonAddress
                {
                    Id = id,
                    PersonId = personId,
                    AddressTypeId = AddressTypeId ?? SpecialValues.NonExistingId,
                    Apartment = Appartment ?? string.Empty,
                    BeginDateTime = FromDate.GetValueOrDefault(SpecialValues.MinDate),
                    EndDateTime = ToDate.GetValueOrDefault(SpecialValues.MaxDate),
                    Building = Building ?? string.Empty,
                    House = House ?? string.Empty,
                    OkatoId = Location == null 
                                    ? Region == null
                                        ? SpecialValues.NonExistingId
                                        : Region.Id
                                    : Location.Id,
                    UserText = UserText ?? string.Empty
                };
            }
            set
            {
                ChangeTracker.IsEnabled = false;
                if (value == null)
                {
                    addressTypeId = null;
                    appartment = string.Empty;
                    building = string.Empty;
                    house = string.Empty;
                    region = null;
                    location = null;
                    userText = string.Empty;
                    fromDate = null;
                    ToDate = null;
                    id = SpecialValues.NewId;
                    personId = SpecialValues.NewId;
                }
                else
                {
                    addressTypeId = value.AddressTypeId;
                    appartment = value.Apartment;
                    building = value.Building;
                    house = value.House;
                    var okato = cacheService.GetItemById<Okato>(value.OkatoId);
                    if (okato == null)
                    {
                        location = null;
                        region = null;
                    }
                    else if (okato.IsRegion || okato.IsForeignCountry)
                    {
                        location = null;
                        region = okato;
                    }
                    else
                    {
                        location = okato;
                        var regionCode = location.CodeOKATO.Substring(0, 2);
                        region = cacheService.GetItems<Okato>().First(x => x.CodeOKATO.StartsWith(regionCode) && x.IsRegion && location.FullName.StartsWith(x.FullName));
                    }
                    userText = value.UserText;
                    fromDate = value.BeginDateTime;
                    toDate = value.EndDateTime == SpecialValues.MaxDate ? null : (DateTime?)value.EndDateTime;
                    id = value.Id;
                    personId = value.PersonId;
                }
                OnPropertyChanged(string.Empty);
                ChangeTracker.IsEnabled = true;
            }
        }

        public bool IsActive
        {
            get { return FromDate.GetValueOrDefault(SpecialValues.MinDate) <= DateTime.Today && DateTime.Today <= ToDate.GetValueOrDefault(SpecialValues.MaxDate); }
        }

        public IEnumerable<AddressType> AddressTypes { get; private set; }

        public void Dispose()
        {
            ChangeTracker.Dispose();
        }

        public ICommand DeleteCommand { get; private set; }

        private void Delete()
        {
            OnDeleteRequested();
        }

        public event EventHandler DeleteRequested;

        protected virtual void OnDeleteRequested()
        {
            var handler = DeleteRequested;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public IChangeTracker ChangeTracker { get; private set; }

        public string StringRepresentation
        {
            get
            {
                if (addressTypeId == null)
                {
                    return string.Empty;
                }
                var result = new StringBuilder();
                result.Append(cacheService.GetItemById<AddressType>(addressTypeId.Value).Name);
                if (!string.IsNullOrWhiteSpace(userText))
                {
                    result.Append(": ")
                          .Append(userText);
                    if (!string.IsNullOrWhiteSpace(house))
                    {
                        result.Append(" д.")
                              .Append(house.Trim());
                        if (!string.IsNullOrWhiteSpace(building))
                        {
                            result.Append('/')
                                  .Append(building.Trim());
                        }
                        if (!string.IsNullOrWhiteSpace(appartment))
                        {
                            result.Append("кв.")
                                  .Append(appartment);
                        }
                    }
                }
                if (fromDate.HasValue)
                {
                    if (toDate.HasValue && toDate.Value > DateTime.Today)
                    {
                        result.Append("НЕ АКТУАЛЕН");
                    }
                    else
                    {
                        result.Append(". Актуален с ")
                            .Append(fromDate.Value.ToString(DateTimeFormats.ShortDateFormat));
                        if (toDate.HasValue)
                        {
                            result.Append(" по ")
                                    .Append(toDate.Value.ToString(DateTimeFormats.ShortDateFormat));
                        }
                    }
                }
                return result.ToString();
            }
        }

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

        private class ValidationMediator : ValidationMediator<AddressViewModel>
        {
            public ValidationMediator(AddressViewModel associatedItem)
                : base(associatedItem)
            {
            }

            protected override void OnValidateProperty(string propertyName)
            {
                if (string.CompareOrdinal(propertyName, "AddressTypeId") == 0)
                {
                    ValidateAddressType();
                }
                else if (string.CompareOrdinal(propertyName, "FromDate") == 0)
                {
                    ValidateFromDate();
                }
                else if (string.CompareOrdinal(propertyName, "Region") == 0)
                {
                    ValidateRegion();
                }
                else if (string.CompareOrdinal(propertyName, "UserText") == 0)
                {
                    ValidateUserText();
                }
                else if (string.CompareOrdinal(propertyName, "House") == 0)
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
                ValidateAddressType();
                ValidateFromDate();
            }

            private void ValidateHouse()
            {
                if (AssociatedItem.Region == null)
                {
                    Errors["House"] = "Укажите номер дома";
                }
                else if (ValidationIsActive)
                {
                    Errors["House"] = string.Empty;
                }
            }

            private void ValidateUserText()
            {
                if (AssociatedItem.Region == null)
                {
                    Errors["UserText"] = "Укажите адрес по документу";
                }
                else if (ValidationIsActive)
                {
                    Errors["UserText"] = string.Empty;
                }
            }

            private void ValidateRegion()
            {
                if (AssociatedItem.Region == null)
                {
                    Errors["Region"] = "Выберите регион или иностранное государство";
                }
                else if (ValidationIsActive)
                {
                    Errors["Region"] = string.Empty;
                }
            }

            private void ValidateFromDate()
            {
                if (AssociatedItem.FromDate == null)
                {
                    Errors["FromDate"] = "Не указана дата начала действия";
                }
                else if (ValidationIsActive)
                {
                    Errors["FromDate"] = string.Empty;
                }
            }

            private void ValidateAddressType()
            {
                if (AssociatedItem.AddressTypeId == null)
                {
                    Errors["AddressTypeId"] = "Не указан тип адреса";
                }
                else if (ValidationIsActive)
                {
                    Errors["AddressTypeId"] = string.Empty;
                }
            }
        }

        #endregion
    }
}
