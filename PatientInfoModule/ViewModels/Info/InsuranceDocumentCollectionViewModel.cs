using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Core.Data;
using Core.Extensions;
using Core.Misc;
using Core.Wpf.Mvvm;
using Prism.Commands;
using Prism.Mvvm;

namespace PatientInfoModule.ViewModels
{
    public class InsuranceDocumentCollectionViewModel : BindableBase, IDisposable, IChangeTrackerMediator, IActiveDataErrorInfo
    {
        private readonly Func<InsuranceDocumentViewModel> insuranceDocumentFactory;

        private readonly CompositeChangeTracker changeTracker;

        public InsuranceDocumentCollectionViewModel(Func<InsuranceDocumentViewModel> insuranceDocumentFactory)
        {
            if (insuranceDocumentFactory == null)
            {
                throw new ArgumentNullException("insuranceDocumentFactory");
            }
            this.insuranceDocumentFactory = insuranceDocumentFactory;
            InsuranceDocuments = new ObservableCollectionEx<InsuranceDocumentViewModel>();
            InsuranceDocuments.BeforeCollectionChanged += OnBeforeInsuranceDocumentsCollectionChanged;
            InsuranceDocuments.CollectionChanged += OnInsuranceDocumentsCollectionChanged;
            changeTracker = new CompositeChangeTracker(new ObservableCollectionChangeTracker<InsuranceDocumentViewModel>(InsuranceDocuments));
            AddNewInsruanceDocumentCommand = new DelegateCommand(AddNewInsuranceDocument);
        }

        private void OnInsuranceDocumentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            OnPropertyChanged(() => StringRepresentation);
        }

        public ICommand AddNewInsruanceDocumentCommand { get; private set; }

        private void AddNewInsuranceDocument()
        {
            var newInsuranceDocument = insuranceDocumentFactory();
            InsuranceDocuments.Add(newInsuranceDocument);
        }

        private void OnBeforeInsuranceDocumentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<InsuranceDocumentViewModel>())
                {
                    newItem.DeleteRequested += OnInsuranceDocumentDeleteRequested;
                    newItem.PropertyChanged += OnInsuranceDocumentPropertyChanged;
                    changeTracker.AddTracker(newItem.ChangeTracker);
                }
            }
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.Cast<InsuranceDocumentViewModel>())
                {
                    oldItem.DeleteRequested -= OnInsuranceDocumentDeleteRequested;
                    oldItem.PropertyChanged -= OnInsuranceDocumentPropertyChanged;
                    changeTracker.RemoveTracker(oldItem.ChangeTracker);
                }
            }
        }

        private void OnInsuranceDocumentPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (string.IsNullOrEmpty(propertyChangedEventArgs.PropertyName) || string.CompareOrdinal(propertyChangedEventArgs.PropertyName, "StringRepresentation") == 0)
            {
                OnPropertyChanged(() => StringRepresentation);
            }
        }

        public ObservableCollectionEx<InsuranceDocumentViewModel> InsuranceDocuments { get; private set; }

        public ICollection<InsuranceDocument> Model
        {
            get { return InsuranceDocuments.Select(x => x.Model).ToArray(); }
            set
            {
                value = value ?? new InsuranceDocument[0];
                ChangeTracker.IsEnabled = false;
                InsuranceDocuments.Clear();
                foreach (var newModel in value)
                {
                    var newDocument = insuranceDocumentFactory();
                    newDocument.Model = newModel;
                    InsuranceDocuments.Add(newDocument);
                }
                ChangeTracker.IsEnabled = true;
            }
        }

        public void Dispose()
        {
            ChangeTracker.Dispose();
            foreach (var insuranceDocument in InsuranceDocuments)
            {
                insuranceDocument.DeleteRequested -= OnInsuranceDocumentDeleteRequested;
                insuranceDocument.PropertyChanged -= OnInsuranceDocumentPropertyChanged;
            }
            InsuranceDocuments.BeforeCollectionChanged -= OnBeforeInsuranceDocumentsCollectionChanged;
            InsuranceDocuments.CollectionChanged -= OnInsuranceDocumentsCollectionChanged;
        }

        private void OnInsuranceDocumentDeleteRequested(object sender, EventArgs e)
        {
            InsuranceDocuments.Remove(sender as InsuranceDocumentViewModel);
        }

        public IChangeTracker ChangeTracker
        {
            get { return changeTracker; }
        }

        public string StringRepresentation
        {
            get
            {
                var documentsRepresentations = InsuranceDocuments.Select(x => x.StringRepresentation)
                                                                 .Where(x => !string.IsNullOrEmpty(x))
                                                                 .ToArray();
                if (documentsRepresentations.Length == 0)
                {
                    return string.Empty;
                }
                if (documentsRepresentations.Length == 1)
                {
                    return documentsRepresentations[0];
                }
                var result = new StringBuilder();
                var index = 1;
                foreach (var documentsRepresentation in documentsRepresentations)
                {
                    result.Append(index)
                          .Append(". ")
                          .AppendLine(documentsRepresentation);
                    index++;
                }
                return result.ToString();
            }
        }

        public string this[string columnName]
        {
            get
            {
                if (string.CompareOrdinal(columnName, "StringRepresentation") == 0)
                {
                    return InsuranceDocuments.Select(x => x.Error).FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? string.Empty;
                }
                return string.Empty;
            }
        }

        public string Error { get { throw new NotImplementedException(); } }

        public bool Validate()
        {
            var result = InsuranceDocuments.Select(x => x.Validate()).ToArray();
            return result.All(x => x);
        }

        public void CancelValidation()
        {
            InsuranceDocuments.ForEach(x => x.CancelValidation());
        }
    }
}
