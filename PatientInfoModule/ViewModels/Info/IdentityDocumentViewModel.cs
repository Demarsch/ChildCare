using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using Core.Misc;
using Core.Services;
using Microsoft.Practices.Unity;
using PatientInfoModule.Misc;
using Prism.Commands;
using Prism.Mvvm;
using WpfControls.Editors;

namespace PatientInfoModule.ViewModels
{
    public class IdentityDocumentViewModel : BindableBase, IDisposable
    {
        private readonly ChangeTracker changeTracker;

        public IdentityDocumentViewModel(ICacheService cacheService)
        {
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            DocumentTypes = cacheService.GetItems<IdentityDocumentType>();
            changeTracker = new ChangeTracker();
            DeleteCommand = new DelegateCommand(Delete);
            changeTracker.PropertyChanged += OnChangesTracked;
        }

        private void OnChangesTracked(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || string.CompareOrdinal(e.PropertyName, "HasChanges") == 0)
            {
                OnPropertyChanged(() => HasChanges);
            }
        }

        public bool HasChanges { get { return changeTracker.HasChanges; } }

        public void CancelChanges()
        {
            changeTracker.Untrack(ref documentTypeId, () => DocumentTypeId);
            changeTracker.Untrack(ref series, () => Series);
            changeTracker.Untrack(ref number, () => Number);
            changeTracker.Untrack(ref givenOrg, () => GivenOrg);
            changeTracker.Untrack(ref givenOrgText, () => GivenOrgText);
            changeTracker.Untrack(ref fromDate, () => FromDate);
            changeTracker.Untrack(ref toDate, () => ToDate);
        }

        private ISuggestionProvider givenOrgSuggestionProvider;

        [Dependency(SuggestionProviderNames.IdentityDocumentGivenOrg)]
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
                changeTracker.Track(documentTypeId, value);
                SetProperty(ref documentTypeId, value);
            }
        }

        private string series;

        public string Series
        {
            get { return series; }
            set
            {
                changeTracker.Track(series, value);
                SetProperty(ref series, value);
            }
        }

        private string number;

        public string Number
        {
            get { return number; }
            set
            {
                changeTracker.Track(number, value);
                SetProperty(ref number, value);
            }
        }

        private string givenOrg;

        public string GivenOrg
        {
            get { return givenOrg; }
            set
            {
                changeTracker.Track(givenOrg, value);
                SetProperty(ref givenOrg, value);
            }
        }

        private string givenOrgText;

        public string GivenOrgText
        {
            get { return givenOrgText; }
            set
            {
                changeTracker.Track(givenOrgText, value);
                SetProperty(ref givenOrgText, value);
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
                changeTracker.Track(fromDate, value);
                if (SetProperty(ref fromDate, value))
                {
                    OnPropertyChanged(() => IsActive);
                    if (FromDate.GetValueOrDefault(SpecialValues.MinDate) > ToDate.GetValueOrDefault(SpecialValues.MaxDate))
                    {
                        ToDate = FromDate;
                    }
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
                changeTracker.Track(toDate, value);
                if (SetProperty(ref toDate, value))
                {
                    HasToDate = value.HasValue;
                    OnPropertyChanged(() => IsActive);
                    if (FromDate.GetValueOrDefault(SpecialValues.MinDate) > ToDate.GetValueOrDefault(SpecialValues.MaxDate))
                    {
                        FromDate = ToDate;
                    }
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

        private PersonIdentityDocument model;

        public PersonIdentityDocument Model
        {
            get
            {
                if (model == null)
                {
                    model = new PersonIdentityDocument();
                }
                model.IdentityDocumentTypeId = DocumentTypeId ?? SpecialValues.NonExistingId;
                model.Series = Series;
                model.Number = Number;
                model.GivenOrg = GivenOrg ?? GivenOrgText;
                model.BeginDate = FromDate ?? SpecialValues.MinDate;
                model.EndDate = ToDate ?? SpecialValues.MaxDate;
                return model;
            }
            set
            {
                changeTracker.IsEnabled = false;
                if (value == null)
                {
                    DocumentTypeId = null;
                    Series = string.Empty;
                    Number = string.Empty;
                    GivenOrg = null;
                    GivenOrgText = string.Empty;
                    FromDate = null;
                    ToDate = null;
                }
                else
                {
                    DocumentTypeId = value.IdentityDocumentTypeId;
                    Series = value.Series;
                    Number = value.Number;
                    GivenOrgText = value.GivenOrg;
                    FromDate = value.BeginDate;
                    ToDate = value.EndDate;
                }
                changeTracker.IsEnabled = true;
                model = value;
            }
        }

        public bool IsActive
        {
            get { return FromDate.GetValueOrDefault(SpecialValues.MinDate) <= DateTime.Today && DateTime.Today <= ToDate.GetValueOrDefault(SpecialValues.MaxDate); }
        }

        public IEnumerable<IdentityDocumentType> DocumentTypes { get; private set; } 

        public void Dispose()
        {
            changeTracker.PropertyChanged -= OnChangesTracked;
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
    }
}
