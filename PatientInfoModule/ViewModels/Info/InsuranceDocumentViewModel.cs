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
using Microsoft.Practices.Unity;
using PatientInfoModule.Misc;
using Prism.Commands;
using WpfControls.Editors;

namespace PatientInfoModule.ViewModels
{
    public class InsuranceDocumentViewModel : TrackableBindableBase, IDisposable, IChangeTrackerMediator, IActiveDataErrorInfo
    {
        private readonly ICacheService cacheService;

        private readonly ValidationMediator validator;

        public InsuranceDocumentViewModel(ICacheService cacheService)
        {
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.cacheService = cacheService;
            validator = new ValidationMediator(this);
            DocumentTypes = cacheService.GetItems<InsuranceDocumentType>();
            DeleteCommand = new DelegateCommand(Delete);
            ChangeTracker = new ChangeTrackerEx<InsuranceDocumentViewModel>(this);
        }

        private ISuggestionProvider insuranceCompanySuggestionProvider;

        [Dependency(SuggestionProviderNames.InsuranceCompany)]
        public ISuggestionProvider InsuranceCompanySuggestionProvider
        {
            get { return insuranceCompanySuggestionProvider; }
            set { SetProperty(ref insuranceCompanySuggestionProvider, value); }
        }

        private int? documentTypeId;

        public int? DocumentTypeId
        {
            get { return documentTypeId; }
            set
            {
                if (SetTrackedProperty(ref documentTypeId, value))
                {
                    OnPropertyChanged(() => StringRepresentation);
                }
            }
        }

        private string series;

        public string Series
        {
            get { return series; }
            set
            {
                if (SetTrackedProperty(ref series, value))
                {
                    OnPropertyChanged(() => StringRepresentation);
                }
            }
        }

        private string number;

        public string Number
        {
            get { return number; }
            set
            {
                if (SetTrackedProperty(ref number, value))
                {
                    OnPropertyChanged(() => StringRepresentation);
                }
            }
        }

        private InsuranceCompany insuranceCompany;

        public InsuranceCompany InsuranceCompany
        {
            get { return insuranceCompany; }
            set
            {
                if (SetTrackedProperty(ref insuranceCompany, value))
                {
                    OnPropertyChanged(() => StringRepresentation);
                }
            }
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

        public InsuranceDocument Model
        {
            get
            {
                return new InsuranceDocument
                {
                    Id = id,
                    PersonId = personId,
                    InsuranceCompanyId = InsuranceCompany == null ? SpecialValues.NonExistingId : InsuranceCompany.Id,
                    InsuranceDocumentTypeId = DocumentTypeId ?? SpecialValues.NonExistingId,
                    Series = Series,
                    Number = Number,
                    BeginDate = FromDate.GetValueOrDefault(SpecialValues.MinDate),
                    EndDate = ToDate.GetValueOrDefault(SpecialValues.MaxDate)
                };
            }
            set
            {
                ChangeTracker.IsEnabled = false;
                if (value == null)
                {
                    documentTypeId = null;
                    series = string.Empty;
                    number = string.Empty;
                    insuranceCompany = null;
                    fromDate = null;
                    ToDate = null;
                    id = SpecialValues.NewId;
                    personId = SpecialValues.NewId;
                }
                else
                {
                    documentTypeId = value.InsuranceDocumentTypeId;
                    series = value.Series;
                    number = value.Number;
                    insuranceCompany = cacheService.GetItemById<InsuranceCompany>(value.InsuranceCompanyId);
                    fromDate = value.BeginDate;
                    toDate = value.EndDate == SpecialValues.MaxDate ? null : (DateTime?)value.EndDate;
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

        public IEnumerable<InsuranceDocumentType> DocumentTypes { get; private set; }

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
                if (documentTypeId == null)
                {
                    return string.Empty;
                }
                var result = new StringBuilder();
                var insuranceDocumentType = cacheService.GetItemById<InsuranceDocumentType>(documentTypeId.Value);
                result.Append(insuranceDocumentType.Name);
                if (!string.IsNullOrWhiteSpace(series))
                {
                    result.Append(' ')
                          .Append(series);
                }
                if (!string.IsNullOrWhiteSpace(number))
                {
                    result.Append(' ')
                          .Append(number);
                }
                if (insuranceCompany != null || fromDate != null)
                {
                    result.Append(' ')
                        .Append(TryGetGrammaticallyProperWord(insuranceDocumentType));
                    if (insuranceCompany != null)
                    {
                        result.Append(' ')
                              .Append(insuranceCompany.NameSMOK);
                    }
                    if (fromDate != null)
                    {
                        result.Append(' ')
                              .Append(fromDate.Value.ToString(DateTimeFormats.ShortDateFormat));
                    }
                }
                if (toDate != null)
                {
                    if (toDate.Value > DateTime.Today)
                    {

                        result.Append(". Действителен до ")
                            .Append(toDate.Value.ToString(DateTimeFormats.ShortDateFormat));
                    }
                    else
                    {
                        result.Append(". НЕДЕЙСТВИТЕЛЕН");
                    }
                }
                return result.ToString();
            }
        }

        private string TryGetGrammaticallyProperWord(InsuranceDocumentType insuranceDocumentType)
        {
            const string result = "выдан";
            var firstWord = insuranceDocumentType.Name.Split(' ').First();
            if (firstWord.EndsWith("о", StringComparison.CurrentCultureIgnoreCase)
                || firstWord.EndsWith("е", StringComparison.CurrentCultureIgnoreCase))
            {
                return "выдано";
            }
            if (firstWord.EndsWith("а", StringComparison.CurrentCultureIgnoreCase)
                || firstWord.EndsWith("я", StringComparison.CurrentCultureIgnoreCase))
            {
                return "выдана";
            }
            return result;
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

        private class ValidationMediator : ValidationMediator<InsuranceDocumentViewModel>
        {
            public ValidationMediator(InsuranceDocumentViewModel associatedItem)
                : base(associatedItem)
            {
            }

            protected override void OnValidateProperty(string propertyName)
            {
                if (PropertyNameEquals(propertyName, x => x.Series) || PropertyNameEquals(propertyName, x => x.Number))
                {
                    ValidateSeriesAndNumber();
                }
                else if (PropertyNameEquals(propertyName, x => x.DocumentTypeId))
                {
                    ValidateDocumentType();
                }
                else if (PropertyNameEquals(propertyName, x => x.FromDate))
                {
                    ValidateFromDate();
                }
                else if (PropertyNameEquals(propertyName, x => x.InsuranceCompany))
                {
                    ValidateInsuranceCompany();
                }
            }

            protected override void RaiseAssociatedObjectPropertyChanged()
            {
                AssociatedItem.OnPropertyChanged(string.Empty);
            }

            protected override void OnValidate()
            {
                ValidateSeriesAndNumber();
                ValidateDocumentType();
                ValidateFromDate();
                ValidateInsuranceCompany();
            }

            private void ValidateInsuranceCompany()
            {
                SetError(x => x.InsuranceCompany, AssociatedItem.InsuranceCompany == null ? "Не указана выдавшая организация" : string.Empty);
            }

            private void ValidateFromDate()
            {
                SetError(x => x.FromDate, AssociatedItem.FromDate == null ? "Не указана дата выдачи" : string.Empty);
            }

            private void ValidateSeriesAndNumber()
            {
                var error = string.IsNullOrWhiteSpace(AssociatedItem.Series) && string.IsNullOrWhiteSpace(AssociatedItem.Number) ? "Серия и номер не могут быть пустыми одновременно" : string.Empty;
                SetError(x => x.Series, error);
                SetError(x => x.Number, error);
            }

            private void ValidateDocumentType()
            {
                SetError(x => x.DocumentTypeId, AssociatedItem.DocumentTypeId == null ? "Не указан тип документа" : string.Empty);
            }
        }

        #endregion
    }
}
