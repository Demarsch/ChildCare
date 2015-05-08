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
    public class PersonIdentityDocumentViewModel : ObservableObject
    {
        #region Fields

        private readonly PersonIdentityDocument personIdentityDocument;

        private IPersonService service;

        #endregion

        #region Constructors

        public PersonIdentityDocumentViewModel(IPersonService service, PersonIdentityDocument personIdentityDocument)
        {
            if (personIdentityDocument == null)
                throw new ArgumentNullException("personIdentityDocument");
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.personIdentityDocument = personIdentityDocument;
            GivenOrgSuggestionProvider = new GivenOrgSuggestionProvider(service);
            FillData();
        }

        #endregion

        #region Methods

        private void FillData()
        {
            if (!IsEmpty)
            {
                IdentityDocumentTypeId = personIdentityDocument.IdentityDocumentTypeId;
                Series = personIdentityDocument.Series;
                Number = personIdentityDocument.Number;
                GivenOrg = personIdentityDocument.GivenOrg;
                BeginDate = personIdentityDocument.BeginDate;
                EndDate = personIdentityDocument.EndDate;
                WithoutEndDate = personIdentityDocument.EndDate.Date == DateTime.MaxValue.Date;
            }
            else
            {
                IdentityDocumentTypeId = 0;
                Series = string.Empty;
                Number = string.Empty;
                GivenOrg = string.Empty;
                BeginDate = new DateTime(1900, 1, 1);
                EndDate = DateTime.MaxValue.Date;
                WithoutEndDate = true;
            }
        }

        #endregion

        #region Properties

        private GivenOrgSuggestionProvider givenOrgSuggestionProvider;
        public GivenOrgSuggestionProvider GivenOrgSuggestionProvider
        {
            get { return givenOrgSuggestionProvider; }
            set { Set("GivenOrgSuggestionProvider", ref givenOrgSuggestionProvider, value); }
        }

        public bool IsEmpty
        {
            get { return personIdentityDocument == null; }
        }

        private int identityDocumentTypeId = 0;
        public int IdentityDocumentTypeId
        {
            get { return identityDocumentTypeId; }
            set { Set("IdentityDocumentTypeId", ref identityDocumentTypeId, value); }
        }

        private string series = string.Empty;
        public string Series
        {
            get { return series; }
            set { Set("Series", ref series, value); }
        }

        private string number = string.Empty;
        public string Number
        {
            get { return number; }
            set { Set("Number", ref number, value); }
        }

        private string givenOrg = string.Empty;
        public string GivenOrg
        {
            get { return givenOrg; }
            set { Set("GivenOrg", ref givenOrg, value); }
        }

        private string givenOrgText = string.Empty;
        public string GivenOrgText
        {
            get { return givenOrgText; }
            set { Set("GivenOrgText", ref givenOrgText, value); }
        }

        private DateTime beginDate = DateTime.MinValue;
        public DateTime BeginDate
        {
            get { return beginDate; }
            set
            {
                Set("BeginDate", ref beginDate, value);
                RaisePropertyChanged("PersonIdentityDocumentState");
                RaisePropertyChanged("PersonIdentityDocumentStateString");
            }
        }

        private DateTime endDate = DateTime.MinValue;
        public DateTime EndDate
        {
            get { return endDate; }
            set
            {
                Set("EndDate", ref endDate, value);
                RaisePropertyChanged("PersonIdentityDocumentState");
                RaisePropertyChanged("PersonIdentityDocumentStateString");
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

        public ItemState PersonIdentityDocumentState
        {
            get
            {
                var datetimeNow = DateTime.Now;
                if (datetimeNow >= BeginDate && datetimeNow < EndDate)
                    return ItemState.Active;
                return ItemState.Inactive;
            }
        }

        public string PersonIdentityDocumentStateString
        {
            get
            {
                switch (PersonIdentityDocumentState)
                {
                    case ItemState.Active:
                        return "Действующий документ";
                    case ItemState.Inactive:
                        return "Недействующий документ";
                    default:
                        return string.Empty;
                }
            }
        }

        public string PersonAddressString
        {
            get
            {
                var IdentityDocumentTypeName = string.Empty;
                var IdentityDocumentType = service.GetIdentityDocumentType(IdentityDocumentTypeId);
                if (IdentityDocumentType != null)
                    IdentityDocumentTypeName = IdentityDocumentType.Name;
                return IdentityDocumentTypeName + ": Серия " + Series + " Номер " + Number + "\r\nВыдан " + (GivenOrg != null ? GivenOrg : GivenOrgText) + " " + BeginDate.ToString("dd.MM.yyyy");
            }
        }

        #endregion
    }
}
