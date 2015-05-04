using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Threading.Tasks;

namespace MainLib
{
    public class PersonAddressViewModel : ObservableObject
    {
        private readonly PersonAddress personAddress;

        public PersonAddressViewModel(PersonAddress personAddress)
        {
            if (personAddress == null)
                throw new ArgumentNullException("personAddress");
            this.personAddress = personAddress;
            FillData();
        }

        private void FillData()
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty
        {
            get { return personAddress == null; }
        }

        private int addressTypeId = 0;
        public int AddressTypeId
        {
            get { return addressTypeId; }
            set { Set("AddressTypeId", ref addressTypeId, value); }
        }

        private Okato addressRegionOKATO = null;
        public Okato AddressRegionOKATO
        {
            get { return addressRegionOKATO; }
            set { Set("AddressRegionOKATO", ref addressRegionOKATO, value); }
        }

        private Okato addressOKATO = null;
        public Okato AddressOKATO
        {
            get { return addressOKATO; }
            set { Set("AddressOKATO", ref addressOKATO, value); }
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

        private string bulding = string.Empty;
        public string Bulding
        {
            get { return bulding; }
            set { Set("Bulding", ref bulding, value); }
        }

        private string apartment = string.Empty;
        public string Apartment
        {
            get { return apartment; }
            set { Set("Apartment", ref apartment, value); }
        }
    }
}
