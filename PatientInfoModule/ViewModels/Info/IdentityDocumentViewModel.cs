using System;
using System.Collections.Generic;
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
    public class IdentityDocumentViewModel : TrackableBindableBase, IDisposable, IChangeTrackerMediator, IActiveDataErrorInfo
    {
        private readonly ICacheService cacheService;

        private readonly ValidationMediator validator;

        public IdentityDocumentViewModel(ICacheService cacheService)
        {
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.cacheService = cacheService;
            validator = new ValidationMediator(this);
            DocumentTypes = cacheService.GetItems<IdentityDocumentType>();
            DeleteCommand = new DelegateCommand(Delete);
            ChangeTracker = new ChangeTrackerEx<IdentityDocumentViewModel>(this);
        }

        private ISuggestionProvider givenOrgSuggestionProvider;

        [Dependency(SuggestionProviderNames.IdentityDocumentGiveOrganization)]
        public ISuggestionProvider GivenOrgSuggestionProvider
        {
            get { return givenOrgSuggestionProvider; }
            set { SetProperty(ref givenOrgSuggestionProvider, value); }
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
                ChangeTracker.IsEnabled = false;
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
                ChangeTracker.IsEnabled = true;
            }
        }

        public bool IsActive
        {
            get { return FromDate.GetValueOrDefault(SpecialValues.MinDate) <= DateTime.Today && DateTime.Today <= ToDate.GetValueOrDefault(SpecialValues.MaxDate); }
        }

        public IEnumerable<IdentityDocumentType> DocumentTypes { get; private set; } 

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
                result.Append(cacheService.GetItemById<IdentityDocumentType>(documentTypeId.Value).Name);
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
                    result.Append(" выдан");
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

        private class ValidationMediator : ValidationMediator<IdentityDocumentViewModel>
        {
            public ValidationMediator(IdentityDocumentViewModel associatedItem) : base(associatedItem)
            {
            }

            protected override void OnValidateProperty(string propertyName)
            {
                if (string.CompareOrdinal(propertyName, "Series") == 0 || string.CompareOrdinal(propertyName, "Number") == 0)
                {
                    ValidateSeriesAndNumber();
                }
                else if (string.CompareOrdinal(propertyName, "DocumentTypeId") == 0)
                {
                    ValidateDocumentType();
                }
                else if (string.CompareOrdinal(propertyName, "FromDate") == 0)
                {
                    ValidateFromDate();
                }
                else if (string.CompareOrdinal(propertyName, "GivenOrg") == 0 || string.CompareOrdinal(propertyName, "GivenOrgText") == 0)
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

            private void ValidateGivenOrg()
            {
                if (string.IsNullOrWhiteSpace(AssociatedItem.GivenOrg) && string.IsNullOrWhiteSpace(AssociatedItem.GivenOrgText))
                {
                    Errors["GivenOrg"] = Errors["GivenOrgText"] = "Не указана выдавшая организация";
                }
                else if (ValidationIsActive)
                {
                    Errors["GivenOrg"] = Errors["GivenOrgText"] = string.Empty;
                }
            }

            private void ValidateFromDate()
            {
                if (AssociatedItem.FromDate == null)
                {
                    Errors["FromDate"] = "Не указана дата выдачи";
                }
                else if (ValidationIsActive)
                {
                    Errors["FromDate"] = string.Empty;
                }
            }

            private void ValidateSeriesAndNumber()
            {
                if (string.IsNullOrWhiteSpace(AssociatedItem.Series) && string.IsNullOrWhiteSpace(AssociatedItem.Number))
                {
                    Errors["Series"] = Errors["Number"] = "Серия и номер не могут быть пустыми одновременно";
                }
                else if (ValidationIsActive)
                {
                    Errors["Series"] = Errors["Number"] = string.Empty;
                }
            }

            private void ValidateDocumentType()
            {
                if (AssociatedItem.DocumentTypeId == null)
                {
                    Errors["DocumentTypeId"] = "Не указан тип документа";
                }
                else if (ValidationIsActive)
                {
                    Errors["DocumentTypeId"] = string.Empty;
                }
            }
        }

        #endregion
    }
}
