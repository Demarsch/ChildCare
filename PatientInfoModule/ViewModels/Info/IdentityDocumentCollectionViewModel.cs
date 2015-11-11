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
    public class IdentityDocumentCollectionViewModel : BindableBase, IDisposable, IChangeTrackerMediator, IActiveDataErrorInfo
    {
        private readonly Func<IdentityDocumentViewModel> identityDocumentFactory;

        private readonly CompositeChangeTracker changeTracker;

        public IdentityDocumentCollectionViewModel(Func<IdentityDocumentViewModel> identityDocumentFactory)
        {
            if (identityDocumentFactory == null)
            {
                throw new ArgumentNullException("identityDocumentFactory");
            }
            this.identityDocumentFactory = identityDocumentFactory;
            IdentityDocuments = new ObservableCollectionEx<IdentityDocumentViewModel>();
            IdentityDocuments.BeforeCollectionChanged += OnBeforeIdentityDocumentsCollectionChanged;
            IdentityDocuments.CollectionChanged += OnIdentityDocumentsCollectionChanged;
            changeTracker = new CompositeChangeTracker(new ObservableCollectionChangeTracker<IdentityDocumentViewModel>(IdentityDocuments));
            AddNewIdentityDocumentCommand = new DelegateCommand(AddNewIdentityDocument);
        }

        private void OnIdentityDocumentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            OnPropertyChanged(() => StringRepresentation);
        }

        public ICommand AddNewIdentityDocumentCommand { get; private set; }

        private void AddNewIdentityDocument()
        {
            var newIdentityDocument = identityDocumentFactory();
            IdentityDocuments.Add(newIdentityDocument);
        }

        private void OnBeforeIdentityDocumentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<IdentityDocumentViewModel>())
                {
                    newItem.DeleteRequested += OnIdentityDocumentDeleteRequested;
                    newItem.PropertyChanged += OnIdentityDocumentPropertyChanged;
                    changeTracker.AddTracker(newItem.ChangeTracker);
                }
            }
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.Cast<IdentityDocumentViewModel>())
                {
                    oldItem.DeleteRequested -= OnIdentityDocumentDeleteRequested;
                    oldItem.PropertyChanged -= OnIdentityDocumentPropertyChanged;
                    changeTracker.RemoveTracker(oldItem.ChangeTracker);
                }
            }
        }

        private void OnIdentityDocumentPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (string.IsNullOrEmpty(propertyChangedEventArgs.PropertyName) || string.CompareOrdinal(propertyChangedEventArgs.PropertyName, "StringRepresentation") == 0)
            {
                OnPropertyChanged(() => StringRepresentation);
            }
        }

        public ObservableCollectionEx<IdentityDocumentViewModel> IdentityDocuments { get; private set; }
        
        public ICollection<PersonIdentityDocument> Model
        {
            get { return IdentityDocuments.Select(x => x.Model).ToArray(); }
            set
            {
                ChangeTracker.IsEnabled = false;
                IdentityDocuments.Clear();
                foreach (var newModel in value)
                {
                    var newDocument = identityDocumentFactory();
                    newDocument.Model = newModel;
                    IdentityDocuments.Add(newDocument);
                }
                ChangeTracker.IsEnabled = true;
            }
        }

        public void Dispose()
        {
            ChangeTracker.Dispose();
            foreach (var identityDocument in IdentityDocuments)
            {
                identityDocument.DeleteRequested -= OnIdentityDocumentDeleteRequested;
                identityDocument.PropertyChanged -= OnIdentityDocumentPropertyChanged;
            }
            IdentityDocuments.BeforeCollectionChanged -= OnBeforeIdentityDocumentsCollectionChanged;
            IdentityDocuments.CollectionChanged -= OnIdentityDocumentsCollectionChanged;
        }

        private void OnIdentityDocumentDeleteRequested(object sender, EventArgs e)
        {
            IdentityDocuments.Remove(sender as IdentityDocumentViewModel);
        }

        public IChangeTracker ChangeTracker
        {
            get { return changeTracker; }
        }

        public string StringRepresentation
        {
            get
            {
                var documentsRepresentations = IdentityDocuments.Select(x => x.StringRepresentation)
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
                    return IdentityDocuments.Select(x => x.Error).FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? string.Empty;
                }
                return string.Empty;
            }
        }

        public string Error { get { throw new NotImplementedException(); } }

        public bool Validate()
        {
            var result = IdentityDocuments.Select(x => x.Validate()).ToArray();
            return result.All(x => x);
        }

        public void CancelValidation()
        {
            IdentityDocuments.ForEach(x => x.CancelValidation());
        }
    }
}
