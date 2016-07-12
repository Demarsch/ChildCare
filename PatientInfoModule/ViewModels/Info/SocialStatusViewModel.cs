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
using PatientInfoModule.Services;
using Prism.Commands;

namespace PatientInfoModule.ViewModels
{
    public class SocialStatusViewModel : TrackableBindableBase, IDisposable, IChangeTrackerMediator, IActiveDataErrorInfo
    {
        private readonly ICacheService cacheService;

        private readonly IPatientService patientService;

        private readonly ValidationMediator validator;

        public SocialStatusViewModel(ICacheService cacheService, IPatientService patientService)
        {
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (patientService == null)
            {
                throw new ArgumentNullException("patientService");
            }
            this.cacheService = cacheService;
            this.patientService = patientService;
            validator = new ValidationMediator(this);
            SocialStatusTypes = cacheService.GetItems<SocialStatusType>();
            DeleteCommand = new DelegateCommand(Delete);
            ChangeTracker = new ChangeTrackerEx<SocialStatusViewModel>(this);
        }

        private ISuggestionsProvider organizationSuggestionsProvider;

        [Dependency(SuggestionProviderNames.Organization)]
        public ISuggestionsProvider OrganizationSuggestionsProvider
        {
            get { return organizationSuggestionsProvider; }
            set { SetProperty(ref organizationSuggestionsProvider, value); }
        }

        private SocialStatusType selectedSocialStatusType;

        public SocialStatusType SelectedSocialStatusType
        {
            get { return selectedSocialStatusType; }
            set
            {
                SelectedOrganization = null;
                OrganizationText = string.Empty;
                Position = string.Empty;

                var oldNeedPlace = selectedSocialStatusType != null && selectedSocialStatusType.NeedPlace;
                if (SetTrackedProperty(ref selectedSocialStatusType, value))
                {
                    OnPropertyChanged(() => StringRepresentation);
                    var newNeedPlace = selectedSocialStatusType != null && selectedSocialStatusType.NeedPlace;
                    if (oldNeedPlace && !newNeedPlace)
                    {
                        SelectedOrganization = null;
                        OrganizationText = string.Empty;
                        Position = string.Empty;
                    }
                }
            }
        }

        private Org selectedOrganization;

        public Org SelectedOrganization
        {
            get { return selectedOrganization; }
            set
            {
                if (SetTrackedProperty(ref selectedOrganization, value))
                {
                    OnPropertyChanged(() => StringRepresentation);
                }
            }
        }

        private string organizationText;

        public string OrganizationText
        {
            get { return organizationText; }
            set
            {
                if (SetProperty(ref organizationText, value))
                {
                    OnPropertyChanged(() => StringRepresentation);
                }
            }
        }

        private string position;

        public string Position
        {
            get { return position; }
            set
            {
                if (SetTrackedProperty(ref position, value))
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

        public PersonSocialStatus Model
        {
            get
            {
                var result = new PersonSocialStatus
                       {
                           Id = id,
                           PersonId = personId,
                           Office = Position,
                           SocialStatusTypeId = SelectedSocialStatusType == null ? SpecialValues.NonExistingId : SelectedSocialStatusType.Id,
                           BeginDateTime = FromDate.GetValueOrDefault(SpecialValues.MinDate),
                           EndDateTime = ToDate.GetValueOrDefault(SpecialValues.MaxDate)
                       };
                if (SelectedOrganization == null)
                {
                    if (SelectedSocialStatusType != null && SelectedSocialStatusType.NeedPlace)
                    {
                        result.OrgId = SpecialValues.NewId;
                        result.Org = new Org
                                     {
                                         Id = SpecialValues.NewId,
                                         Details = "Пользовательская организация",
                                         IsLpu = false,
                                         Name = OrganizationText,
                                         UseInContract = false,
                                         BeginDateTime = FromDate.GetValueOrDefault(SpecialValues.MinDate),
                                         EndDateTime = ToDate.GetValueOrDefault(SpecialValues.MaxDate)
                                     };
                    }
                    else
                    {
                        result.OrgId = null;
                    }
                }
                else
                {
                    result.OrgId = SelectedOrganization.Id;
                }
                return result;
            }
            set
            {
                ChangeTracker.IsEnabled = false;
                if (value == null)
                {
                    selectedSocialStatusType = null;
                    selectedOrganization = null;
                    organizationText = string.Empty;
                    position = string.Empty;
                    fromDate = null;
                    ToDate = null;
                    id = SpecialValues.NewId;
                    personId = SpecialValues.NewId;
                }
                else
                {
                    selectedSocialStatusType = cacheService.GetItemById<SocialStatusType>(value.SocialStatusTypeId);
                    selectedOrganization = value.OrgId == null ? null : patientService.GetOrganization(value.OrgId.Value);
                    organizationText = selectedOrganization == null ? string.Empty : selectedOrganization.Name;
                    position = value.Office;
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

        public IEnumerable<SocialStatusType> SocialStatusTypes { get; private set; } 

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
                if (selectedSocialStatusType == null)
                {
                    return string.Empty;
                }
                var result = new StringBuilder();
                result.Append(selectedSocialStatusType.Name);
                if (selectedOrganization != null || !string.IsNullOrWhiteSpace(organizationText))
                {
                    result.Append(". Организация: ");
                    result.Append(selectedOrganization != null ? selectedOrganization.Name : organizationText);
                }
                if (!string.IsNullOrWhiteSpace(position))
                {
                    result.Append(". Должность: ")
                          .Append(position);
                }
                if (fromDate != null)
                {
                    result.Append(" c ")
                          .Append(fromDate.Value.ToString(DateTimeFormats.ShortDateFormat));
                    if (toDate != null)
                    {
                        result.Append(" по ")
                              .Append(toDate.Value.ToString(DateTimeFormats.ShortDateFormat));
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

        private class ValidationMediator : ValidationMediator<SocialStatusViewModel>
        {
            public ValidationMediator(SocialStatusViewModel associatedItem)
                : base(associatedItem)
            {
            }

            protected override void OnValidateProperty(string propertyName)
            {
                if (PropertyNameEquals(propertyName, x => x.Position))
                {
                    ValidatePosition();
                }
                else if (PropertyNameEquals(propertyName, x => x.SelectedSocialStatusType))
                {
                    ValidateSocialStatusType();
                }
                else if (PropertyNameEquals(propertyName, x => x.FromDate))
                {
                    ValidateFromDate();
                }
                else if (PropertyNameEquals(propertyName, x => x.SelectedOrganization)  || PropertyNameEquals(propertyName, x => x.OrganizationText))
                {
                    ValidateOrganization();
                }
            }

            protected override void RaiseAssociatedObjectPropertyChanged()
            {
                AssociatedItem.OnPropertyChanged(string.Empty);
            }

            protected override void OnValidate()
            {
                ValidatePosition();
                ValidateSocialStatusType();
                ValidateFromDate();
                ValidateOrganization();
            }

            private void ValidateOrganization()
            {
                var error = AssociatedItem.SelectedSocialStatusType != null
                            && AssociatedItem.SelectedSocialStatusType.NeedPlace
                            && AssociatedItem.SelectedOrganization == null
                            && string.IsNullOrWhiteSpace(AssociatedItem.OrganizationText)
                    ? "Не указано место работы/учебы"
                    : string.Empty;
                SetError(x => x.SelectedOrganization, error);
                SetError(x => x.OrganizationText, error);
            }

            private void ValidateFromDate()
            {
                SetError(x => x.FromDate, AssociatedItem.FromDate == null ? "Не указана дата начала действия" : string.Empty);
            }

            private void ValidatePosition()
            {
                SetError(x => x.Position, AssociatedItem.SelectedSocialStatusType != null
                                          && AssociatedItem.SelectedSocialStatusType.NeedPlace
                                          && string.IsNullOrWhiteSpace(AssociatedItem.Position)
                                            ? "Не указана должность/позиция"
                                            : string.Empty);
            }

            private void ValidateSocialStatusType()
            {
                SetError(x => x.SelectedSocialStatusType, AssociatedItem.SelectedSocialStatusType == null ? "Не указан тип статуса" : string.Empty);
            }
        }

        #endregion
    }
}
