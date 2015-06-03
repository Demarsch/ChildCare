using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Threading.Tasks;
using Core;
using System.ComponentModel;

namespace Registry
{
    public class PersonIdentityDocumentViewModel : ObservableObject, IDataErrorInfo
    {
        #region Fields

        private PersonIdentityDocument personIdentityDocument;

        private IPersonService service;

        #endregion

        #region Constructors

        public PersonIdentityDocumentViewModel(PersonIdentityDocument personIdentityDocument, IPersonService service)
        {
            if (personIdentityDocument == null)
                throw new ArgumentNullException("personIdentityDocument");
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.personIdentityDocument = personIdentityDocument;
            GivenOrgSuggestionProvider = new IdentityDocumentsGivenOrgSuggestionProvider(service);
            FillData();
        }

        #endregion

        #region Methods

        public PersonIdentityDocument SetData()
        {
            if (personIdentityDocument == null)
                personIdentityDocument = new PersonIdentityDocument();
            personIdentityDocument.IdentityDocumentTypeId = IdentityDocumentTypeId;
            personIdentityDocument.Series = Series;
            personIdentityDocument.Number = Number;
            personIdentityDocument.GivenOrg = GivenOrg;
            personIdentityDocument.BeginDate = BeginDate;
            personIdentityDocument.EndDate = endDate;

            return personIdentityDocument;
        }


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

        private IdentityDocumentsGivenOrgSuggestionProvider givenOrgSuggestionProvider;
        public IdentityDocumentsGivenOrgSuggestionProvider GivenOrgSuggestionProvider
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
                return IdentityDocumentTypeName + ": Серия " + Series + " Номер " + Number + "\r\nВыдан " + (GivenOrg != null ? GivenOrg : GivenOrgText) + " " + BeginDate.ToString("dd.MM.yyyy") + (EndDate != DateTime.MaxValue ? " по " + EndDate.ToString("dd.MM.yyyy") : string.Empty); ;
            }
        }

        #endregion

        #region Implementation IDataErrorInfo

        private bool saveWasRequested { get; set; }

        public readonly HashSet<string> invalidProperties = new HashSet<string>();

        public bool Invalidate()
        {
            saveWasRequested = true;
            RaisePropertyChanged(string.Empty);
            return invalidProperties.Count < 1;
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                if (columnName == "IdentityDocumentTypeId")
                {
                    result = IdentityDocumentTypeId < 1 ? "Укажите тип документа" : string.Empty;
                }
                if (columnName == "Series")
                {
                    result = string.IsNullOrEmpty(Series) ? "Укажите серию документа" : string.Empty;
                }
                if (columnName == "Number")
                {
                    result = string.IsNullOrEmpty(Number) ? "Укажите номер документа" : string.Empty;
                }
                if (columnName == "GivenOrg" || columnName == "GivenOrgText")
                {
                    result = string.IsNullOrEmpty(GivenOrgText) && GivenOrg == null ? "Укажите нвзвание выдавшей организации" : string.Empty;
                }
                if (columnName == "BeginDate" || columnName == "EndDate")
                {
                    result = BeginDate > EndDate ? "Дата начала не может быть больше даты окончания" : string.Empty;
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

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
