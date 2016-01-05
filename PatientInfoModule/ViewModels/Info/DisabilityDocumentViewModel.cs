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

namespace PatientInfoModule.ViewModels
{
    public class DisabilityDocumentViewModel : TrackableBindableBase, IDisposable, IChangeTrackerMediator, IActiveDataErrorInfo
    {
        private readonly ICacheService cacheService;

        private readonly ValidationMediator validator;

        public DisabilityDocumentViewModel(ICacheService cacheService)
        {
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.cacheService = cacheService;
            validator = new ValidationMediator(this);
            DisabilityTypes = cacheService.GetItems<DisabilityType>();
            DeleteCommand = new DelegateCommand(Delete);
            ChangeTracker = new ChangeTrackerEx<DisabilityDocumentViewModel>(this);
        }

        private ISuggestionProvider givenOrgSuggestionProvider;

        [Dependency(SuggestionProviderNames.DisabilityDocumentGivenOrganization)]
        public ISuggestionProvider GivenOrgSuggestionProvider
        {
            get { return givenOrgSuggestionProvider; }
            set { SetProperty(ref givenOrgSuggestionProvider, value); }
        }

        private int? disabilityTypeId;

        public int? DisabilityTypeId
        {
            get { return disabilityTypeId; }
            set
            {
                if (SetTrackedProperty(ref disabilityTypeId, value))
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

        public PersonDisability Model
        {
            get
            {
                return new PersonDisability
                       {
                           Id = id,
                           PersonId = personId,
                           DisabilityTypeId = DisabilityTypeId ?? SpecialValues.NonExistingId,
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
                    disabilityTypeId = null;
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
                    disabilityTypeId = value.DisabilityTypeId;
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

        public IEnumerable<DisabilityType> DisabilityTypes { get; private set; } 

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
                if (disabilityTypeId == null)
                {
                    return string.Empty;
                }
                var result = new StringBuilder();
                result.Append(cacheService.GetItemById<DisabilityType>(disabilityTypeId.Value).Name);
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
                    result.Append(" подтверждено");
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

        private class ValidationMediator : ValidationMediator<DisabilityDocumentViewModel>
        {
            public ValidationMediator(DisabilityDocumentViewModel associatedItem)
                : base(associatedItem)
            {
            }

            protected override void OnValidateProperty(string propertyName)
            {
                if (PropertyNameEquals(propertyName, x => x.Series)  || PropertyNameEquals(propertyName, x => x.Number) )
                {
                    ValidateSeriesAndNumber();
                }
                else if (PropertyNameEquals(propertyName, x => x.DisabilityTypeId) )
                {
                    ValidatetDisabilityType();
                }
                else if (PropertyNameEquals(propertyName, x => x.FromDate) )
                {
                    ValidateFromDate();
                }
                else if (PropertyNameEquals(propertyName, x => x.GivenOrg)  || PropertyNameEquals(propertyName, x => x.GivenOrgText) )
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
                ValidatetDisabilityType();
                ValidateFromDate();
                ValidateGivenOrg();
            }

            private void ValidateGivenOrg()
            {
                var error = string.IsNullOrWhiteSpace(AssociatedItem.GivenOrg) && string.IsNullOrWhiteSpace(AssociatedItem.GivenOrgText) ? "Не указана выдавшая организация" : string.Empty;
                SetError(x => x.GivenOrg, error);
                SetError(x => x.GivenOrgText, error);
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

            private void ValidatetDisabilityType()
            {
                SetError(x => x.DisabilityTypeId, AssociatedItem.DisabilityTypeId == null ? "Не указан тип документа" : string.Empty);
            }
        }

        #endregion
    }
}
