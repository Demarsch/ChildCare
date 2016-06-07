using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using Core.Extensions;
using Core.Misc;
using Core.Services;
using Core.Wpf.Misc;
using Microsoft.Practices.Unity;
using PatientInfoModule.Misc;
using Prism.Commands;

namespace PatientInfoModule.ViewModels
{
    public class IdentityDocumentViewModel : TrackableBindableBase, IDisposable, IChangeTrackerMediator, IActiveDataErrorInfo
    {
        private readonly ICacheService cacheService;

        private ValidationMediatorBase validator;

        public IdentityDocumentViewModel(ICacheService cacheService)
        {
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.cacheService = cacheService;
            validator = new ValidationMediatorBase(this);
            DocumentTypes = cacheService.GetItems<IdentityDocumentType>();
            DeleteCommand = new DelegateCommand(Delete);
            CompositeChangeTracker = new ChangeTrackerEx<IdentityDocumentViewModel>(this);
        }

        private ISuggestionsProvider givenOrgSuggestionsProvider;

        [Dependency(SuggestionProviderNames.IdentityDocumentGiveOrganization)]
        public ISuggestionsProvider GivenOrgSuggestionsProvider
        {
            get { return givenOrgSuggestionsProvider; }
            set { SetProperty(ref givenOrgSuggestionsProvider, value); }
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
                    UpdateValidatorAndInputHelpers();
                }
            }
        }

        private IInputHelper seriesInputHelper;

        public IInputHelper SeriesInputHelper
        {
            get { return seriesInputHelper; }
            private set { SetProperty(ref seriesInputHelper, value); }
        }

        private IInputHelper numberInputHelper;

        public IInputHelper NumberInputHelper
        {
            get { return numberInputHelper; }
            private set { SetProperty(ref numberInputHelper, value); }
        }

        private void UpdateValidatorAndInputHelpers()
        {
            var reValidate = validator.ValidationIsActive;
            var selectedDocumentType = cacheService.GetItemById<IdentityDocumentType>(DocumentTypeId.GetValueOrDefault(SpecialValues.NonExistingId));
            if (selectedDocumentType == null)
            {
                SeriesInputHelper = null;
                NumberInputHelper = null;
                validator = new ValidationMediatorBase(this);
            }
            else if (selectedDocumentType.Options.HasOption(IdentityDocumentType.IsRussianPassportOption))
            {
                SeriesInputHelper = new DigitsInputHelper(IdentityDocumentType.RussianPassportSeriesDigitCount);
                NumberInputHelper = new DigitsInputHelper(IdentityDocumentType.RussianPassportNumberDigitCount);
                validator = new RussianPassportValidationMediator(this);
            }
            else if (selectedDocumentType.Options.HasOption(IdentityDocumentType.IsRussianBirthCertificateOption))
            {
                SeriesInputHelper = new RussianBirthCertificateSeriesInputHelper();
                NumberInputHelper = new DigitsInputHelper(IdentityDocumentType.RussianBirthCertificateNumberDigitCount);
                validator = new RussianBirthCertificateValidationMediator(this);
            }
            else if (selectedDocumentType.Options.HasOption(IdentityDocumentType.IsRussianForeignPassportOption))
            {
                SeriesInputHelper = new DigitsInputHelper(IdentityDocumentType.RussianForeignPassportSeriesDigitCount);
                NumberInputHelper = new DigitsInputHelper(IdentityDocumentType.RussianForeignPassportNumberDigitCount);
                validator = new RussianForeignPassportValidationMediator(this);
            }
            else
            {
                SeriesInputHelper = null;
                NumberInputHelper = null;
                validator = new ValidationMediatorBase(this);
            }
            if (reValidate)
            {
                validator.Validate();
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

        private string givenOrg;

        public string GivenOrg
        {
            get { return givenOrg; }
            set
            {
                if (SetTrackedProperty(ref givenOrg, value))
                {
                    OnPropertyChanged(() => StringRepresentation);
                }
            }
        }

        private string givenOrgText;

        public string GivenOrgText
        {
            get { return givenOrgText; }
            set
            {
                if (SetTrackedProperty(ref givenOrgText, value))
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

        public PersonIdentityDocument Model
        {
            get
            {
                return new PersonIdentityDocument
                       {
                           Id = id,
                           PersonId = personId,
                           IdentityDocumentTypeId = DocumentTypeId ?? SpecialValues.NonExistingId,
                           Series = Series,
                           Number = Number,
                           GivenOrg = GivenOrg ?? GivenOrgText,
                           BeginDate = FromDate.GetValueOrDefault(SpecialValues.MinDate),
                           EndDate = ToDate.GetValueOrDefault(SpecialValues.MaxDate)
                       };
            }
            set
            {
                CompositeChangeTracker.IsEnabled = false;
                if (value == null)
                {
                    documentTypeId = null;
                    series = string.Empty;
                    number = string.Empty;
                    givenOrg = null;
                    givenOrgText = string.Empty;
                    fromDate = null;
                    ToDate = null;
                    id = SpecialValues.NewId;
                    personId = SpecialValues.NewId;
                }
                else
                {
                    documentTypeId = value.IdentityDocumentTypeId;
                    series = value.Series;
                    number = value.Number;
                    givenOrgText = value.GivenOrg;
                    givenOrg = value.GivenOrg;
                    fromDate = value.BeginDate;
                    toDate = value.EndDate == SpecialValues.MaxDate ? null : (DateTime?)value.EndDate;
                    id = value.Id;
                    personId = value.PersonId;
                }
                OnPropertyChanged(string.Empty);
                CompositeChangeTracker.IsEnabled = true;
            }
        }

        public bool IsActive
        {
            get { return FromDate.GetValueOrDefault(SpecialValues.MinDate) <= DateTime.Today && DateTime.Today <= ToDate.GetValueOrDefault(SpecialValues.MaxDate); }
        }

        public IEnumerable<IdentityDocumentType> DocumentTypes { get; private set; } 

        public void Dispose()
        {
            CompositeChangeTracker.Dispose();
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

        public IChangeTracker CompositeChangeTracker { get; private set; }

        public string StringRepresentation
        {
            get
            {
                if (documentTypeId == null)
                {
                    return string.Empty;
                }
                var result = new StringBuilder();
                var identityDocumentType = cacheService.GetItemById<IdentityDocumentType>(documentTypeId.Value);
                result.Append(identityDocumentType.Name);
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
                if (!string.IsNullOrWhiteSpace(givenOrgText) || !string.IsNullOrWhiteSpace(givenOrg) || fromDate != null)
                {
                    result.Append(' ')
                          .Append(TryGetGrammaticallyProperWord(identityDocumentType));
                    if (!string.IsNullOrWhiteSpace(givenOrg))
                    {
                        result.Append(' ')
                              .Append(givenOrg);
                    }
                    else if (!string.IsNullOrWhiteSpace(givenOrgText))
                    {
                        result.Append(' ')
                              .Append(givenOrgText);
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

        private string TryGetGrammaticallyProperWord(IdentityDocumentType identityDocumentType)
        {
            const string result = "выдан";
            var firstWord = identityDocumentType.Name.Split(' ').First();
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

        private class ValidationMediatorBase : ValidationMediator<IdentityDocumentViewModel>
        {
            public ValidationMediatorBase(IdentityDocumentViewModel associatedItem) : base(associatedItem)
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
                else if (PropertyNameEquals(propertyName, x => x.GivenOrg) || PropertyNameEquals(propertyName, x => x.GivenOrgText))
                {
                    ValidateGivenOrg();
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
                ValidateGivenOrg();
            }

            protected virtual void ValidateGivenOrg()
            {
                var error = string.IsNullOrWhiteSpace(AssociatedItem.GivenOrg) && string.IsNullOrWhiteSpace(AssociatedItem.GivenOrgText) ? "Не указана выдавшая организация" : string.Empty;
                SetError(x => x.GivenOrg, error);
                SetError(x => x.GivenOrgText, error);
            }

            protected virtual void ValidateFromDate()
            {
                SetError(x => x.FromDate, AssociatedItem.FromDate == null ? "Не указана дата выдачи" : string.Empty);
            }

            protected virtual void ValidateSeriesAndNumber()
            {
                var error = string.IsNullOrWhiteSpace(AssociatedItem.Series) && string.IsNullOrWhiteSpace(AssociatedItem.Number) ? "Серия и номер не могут быть пустыми одновременно" : string.Empty;
                SetError(x => x.Series, error);
                SetError(x => x.Number, error);
            }

            protected virtual void ValidateDocumentType()
            {
                SetError(x => x.DocumentTypeId, AssociatedItem.DocumentTypeId == null ? "Не указан тип документа" : string.Empty);
            }
        }

        private class RussianPassportValidationMediator : ValidationMediatorBase
        {
            public RussianPassportValidationMediator(IdentityDocumentViewModel associatedItem)
                : base(associatedItem)
            {
            }

            protected override void ValidateSeriesAndNumber()
            {
                ValidateSeries();
                ValidateNumber();
            }

            private void ValidateNumber()
            {
                int number;
                SetError(x => x.Number, string.IsNullOrEmpty(AssociatedItem.Number) || AssociatedItem.Number.Length != 6 || !int.TryParse(AssociatedItem.Number, out number)
                                            ? "Номер паспорта РФ должен содержать ровно шесть цифр"
                                            : string.Empty);
            }

            private void ValidateSeries()
            {
                int series;
                SetError(x => x.Series, string.IsNullOrEmpty(AssociatedItem.Series) || AssociatedItem.Series.Length != 4 || !int.TryParse(AssociatedItem.Series, out series)
                                            ? "Серия паспорта РФ должна содержать ровно четыре цифры"
                                            : string.Empty);
            }
        }

        private class RussianBirthCertificateValidationMediator : ValidationMediatorBase
        {
            public RussianBirthCertificateValidationMediator(IdentityDocumentViewModel associatedItem)
                : base(associatedItem)
            {
            }

            protected override void ValidateSeriesAndNumber()
            {
                ValidateSeries();
                ValidateNumber();
            }

            private void ValidateNumber()
            {
                int number;
                SetError(x => x.Number, string.IsNullOrEmpty(AssociatedItem.Number) || AssociatedItem.Number.Length != 6 || !int.TryParse(AssociatedItem.Number, out number)
                                            ? "Номер свидетельства о рождении РФ должен содержать ровно шесть цифр"
                                            : string.Empty);
            }

            private void ValidateSeries()
            {
                var error = "Формат ввода серии свидетельства о рождении: римские цифры (латиницей), дефис, две буквы кирилицей";
                if (string.IsNullOrEmpty(AssociatedItem.Series))
                {
                    goto SetError;
                }
                var words = AssociatedItem.Series.Split('-');
                if (words.Length != 2)
                {
                    goto SetError;
                }
                if (!words[0].IsRomanNumber())
                {
                    goto SetError;
                }
                if (words[1].Length != 2 || !words[1].All(CharExtensions.IsRussianLetter))
                {
                    goto SetError;
                }
                error = string.Empty;
            SetError:
                SetError(x => x.Series, error);
            }
        }

        private class RussianForeignPassportValidationMediator : ValidationMediatorBase
        {
            public RussianForeignPassportValidationMediator(IdentityDocumentViewModel associatedItem)
                : base(associatedItem)
            {
            }

            protected override void ValidateSeriesAndNumber()
            {
                ValidateSeries();
                ValidateNumber();
            }

            private void ValidateNumber()
            {
                int number;
                SetError(x => x.Number, string.IsNullOrEmpty(AssociatedItem.Number) || AssociatedItem.Number.Length != 7 || !int.TryParse(AssociatedItem.Number, out number)
                                            ? "Номер заграничного паспорта РФ должен содержать ровно семь цифр"
                                            : string.Empty);
            }

            private void ValidateSeries()
            {
                int series;
                SetError(x => x.Series, string.IsNullOrEmpty(AssociatedItem.Series) || AssociatedItem.Series.Length != 2 || !int.TryParse(AssociatedItem.Series, out series)
                                            ? "Серия заграничного паспорта РФ должна содержать ровно две цифры"
                                            : string.Empty);
            }
        }

        #endregion
    }
}
