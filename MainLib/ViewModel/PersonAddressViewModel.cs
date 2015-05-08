using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Threading.Tasks;
using Core;

namespace MainLib
{
    public class PersonAddressViewModel : ObservableObject
    {
        #region Fields

        private readonly PersonAddress personAddress;

        private IPersonService service;

        #endregion

        #region Constructors

        public PersonAddressViewModel(IPersonService service, PersonAddress personAddress)
        {
            if (personAddress == null)
                throw new ArgumentNullException("personAddress");
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.personAddress = personAddress;
            OkatoRegionSuggestionProvider = new OKATORegionSuggestionProvider(service);
            OkatoSuggestionProvider = new OKATOSuggestionProvider(service);
            FillData();
        }

        #endregion

        #region Methods

        private void FillData()
        {
            if (!IsEmpty)
            {
                AddressTypeId = personAddress.AddressTypeId;
                AddressOKATO = service.GetOKATOByCode(personAddress.OkatoText);
                UserText = personAddress.UserText;
                House = personAddress.House;
                Building = personAddress.Building;
                Apartment = personAddress.Apartment;
                BeginDate = personAddress.BeginDateTime;
                EndDate = personAddress.EndDateTime;
                WithoutEndDate = personAddress.EndDateTime.Date == DateTime.MaxValue.Date;
            }
            else
            {
                AddressTypeId = 0;
                AddressOKATO = null;
                UserText = string.Empty;
                House = string.Empty;
                Building = string.Empty;
                Apartment = string.Empty;
                BeginDate = new DateTime(1900, 1, 1);
                EndDate = DateTime.MaxValue.Date;
                WithoutEndDate = true;
            }
        }

        #endregion

        #region Properties

        private OKATOSuggestionProvider okatoSuggestionProvider;
        public OKATOSuggestionProvider OkatoSuggestionProvider
        {
            get { return okatoSuggestionProvider; }
            set { Set("OkatoSuggestionProvider", ref okatoSuggestionProvider, value); }
        }

        private OKATORegionSuggestionProvider okatoRegionSuggestionProvider;
        public OKATORegionSuggestionProvider OkatoRegionSuggestionProvider
        {
            get { return okatoRegionSuggestionProvider; }
            set { Set("OkatoRegionSuggestionProvider", ref okatoRegionSuggestionProvider, value); }
        }

        public bool IsEmpty
        {
            get { return personAddress == null; }
        }

        private int addressTypeId = 0;
        public int AddressTypeId
        {
            get { return addressTypeId; }
            set 
            { 
                Set("AddressTypeId", ref addressTypeId, value);
                var addressType = service.GetAddressType(addressTypeId);
                if (addressType != null)
                    WithoutEndDate = addressType.WithoutEndDate;
            }
        }

        private Okato regionOKATO = null;
        public Okato RegionOKATO
        {
            get { return regionOKATO; }
            set
            {
                Set("RegionOKATO", ref regionOKATO, value);
                var okatoRegion = string.Empty;
                if (regionOKATO != null)
                    okatoRegion = regionOKATO.CodeOKATO;
                AddressOKATO = null;
                OkatoSuggestionProvider.OkatoRegion = okatoRegion;
                if (AddressOKATO == null && RegionOKATO != null)
                    UserText = RegionOKATO.FullName;
            }
        }

        private Okato addressOKATO = null;
        public Okato AddressOKATO
        {
            get { return addressOKATO; }
            set
            { 
                Set("AddressOKATO", ref addressOKATO, value);
                if (addressOKATO != null)
                    UserText = addressOKATO.FullName;
            }
        }

        private string userText = string.Empty;
        public string UserText
        {
            get { return userText; }
            set { Set("UserText", ref userText, value); }
        }

        private string house = string.Empty;
        public string House
        {
            get { return house; }
            set { Set("House", ref house, value); }
        }

        private string building = string.Empty;
        public string Building
        {
            get { return building; }
            set { Set("Building", ref building, value); }
        }

        private string apartment = string.Empty;
        public string Apartment
        {
            get { return apartment; }
            set { Set("Apartment", ref apartment, value); }
        }

        private DateTime beginDate = DateTime.MinValue;
        public DateTime BeginDate
        {
            get { return beginDate; }
            set
            {
                Set("BeginDate", ref beginDate, value);
                RaisePropertyChanged("PersonAddressState");
                RaisePropertyChanged("PersonAddressStateString");
            }
        }

        private DateTime endDate = DateTime.MinValue;
        public DateTime EndDate
        {
            get { return endDate; }
            set
            {
                Set("EndDate", ref endDate, value);
                RaisePropertyChanged("PersonAddressState");
                RaisePropertyChanged("PersonAddressStateString");
            }
        }

        private bool withoutEndDate;
        public bool WithoutEndDate
        {
            get { return withoutEndDate; }
            set
            {
                Set("WithoutEndDate", ref withoutEndDate, value);
                EndDate = DateTime.MaxValue;
                RaisePropertyChanged("WithEndDate");
            }
        }

        public bool WithEndDate
        {
            get { return !WithoutEndDate; }
        }

        public ItemState PersonAddressState
        {
            get
            {
                var datetimeNow = DateTime.Now;
                if (datetimeNow >= BeginDate && datetimeNow < EndDate)
                    return ItemState.Active;
                return ItemState.Inactive;
            }
        }

        public string PersonAddressStateString
        {
            get
            {
                switch (PersonAddressState)
                {
                    case ItemState.Active:
                        return "Действующий адрес";
                    case ItemState.Inactive:
                        return "Недействующий адрес";
                    default:
                        return string.Empty;
                }
            }
        }

        public string PersonAddressString
        {
            get
            {
                var addressTypeName = string.Empty;
                var addressType = service.GetAddressType(AddressTypeId);
                if (addressType != null)
                    addressTypeName = addressType.Name;
                return addressTypeName + ": " + UserText + " " + House + (Building != string.Empty ? "\"" + Building + "\"" : string.Empty) +
                    (Apartment != string.Empty ? " " + Apartment : string.Empty) + "\r\nДействует с " + BeginDate.ToString("dd.MM.yyyy") + (EndDate != DateTime.MaxValue ? " по" + EndDate.ToString("dd.MM.yyyy") : string.Empty);
            }
        }

        #endregion
    }
}
