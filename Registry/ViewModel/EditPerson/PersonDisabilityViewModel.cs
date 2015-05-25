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
    public class PersonDisabilityViewModel : ObservableObject, IDataErrorInfo
    {
        #region Fields

        private readonly PersonDisability personDisability;

        private IPersonService service;

        #endregion

        #region Constructors

        public PersonDisabilityViewModel(PersonDisability personDisability, IPersonService service)
        {
            if (personDisability == null)
                throw new ArgumentNullException("personDisability");
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.personDisability = personDisability;
            GivenOrgSuggestionProvider = new DisabilitiesGivenOrgSuggestionProvider(service);
            FillData();
        }

        #endregion

        #region Methods

        private void FillData()
        {
            if (!IsEmpty)
            {
                DisabilityTypeId = personDisability.DisabilityTypeId;
                Series = personDisability.Series;
                Number = personDisability.Number;
                GivenOrg = personDisability.GivenOrg;
                BeginDate = personDisability.BeginDate;
                EndDate = personDisability.EndDate;
                WithoutEndDate = personDisability.EndDate.Date == DateTime.MaxValue.Date;
            }
            else
            {
                DisabilityTypeId = 0;
                Series = string.Empty;
                Number = string.Empty;
                GivenOrg = string.Empty;
                BeginDate = new DateTime(1900, 1, 1);
                EndDate = DateTime.MaxValue.Date;
                //WithoutEndDate = true;
            }
        }

        #endregion

        #region Properties

        private DisabilitiesGivenOrgSuggestionProvider givenOrgSuggestionProvider;
        public DisabilitiesGivenOrgSuggestionProvider GivenOrgSuggestionProvider
        {
            get { return givenOrgSuggestionProvider; }
            set { Set("GivenOrgSuggestionProvider", ref givenOrgSuggestionProvider, value); }
        }

        public bool IsEmpty
        {
            get { return personDisability == null; }
        }

        private int disabilityTypeId = 0;
        public int DisabilityTypeId
        {
            get { return disabilityTypeId; }
            set { Set("DisabilityTypeId", ref disabilityTypeId, value); }
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
                RaisePropertyChanged("PersonDisabilityState");
                RaisePropertyChanged("PersonDisabilityStateString");
            }
        }

        private DateTime endDate = DateTime.MinValue;
        public DateTime EndDate
        {
            get { return endDate; }
            set
            {
                Set("EndDate", ref endDate, value);
                RaisePropertyChanged("PersonDisabilityState");
                RaisePropertyChanged("PersonDisabilityStateString");
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

        public ItemState PersonDisabilityState
        {
            get
            {
                var datetimeNow = DateTime.Now;
                if (datetimeNow >= BeginDate && datetimeNow < EndDate)
                    return ItemState.Active;
                return ItemState.Inactive;
            }
        }

        public string PersonDisabilityStateString
        {
            get
            {
                switch (PersonDisabilityState)
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

        public string PersonDisabilityString
        {
            get
            {
                var disabilityTypeName = string.Empty;
                var disabilityType = service.GetDisabilityType(DisabilityTypeId);
                if (disabilityType != null)
                    disabilityTypeName = disabilityType.Name;
                return disabilityTypeName + ": Серия " + Series + " Номер " + Number + "\r\nВыдан " + (GivenOrg != null ? GivenOrg : GivenOrgText) + " " + BeginDate.ToString("dd.MM.yyyy") + (EndDate != DateTime.MaxValue ? " по " + EndDate.ToString("dd.MM.yyyy") : string.Empty);
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
                if (columnName == "DisabilityTypeId")
                {
                    result = DisabilityTypeId < 1 ? "Укажите тип документа" : string.Empty;
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
