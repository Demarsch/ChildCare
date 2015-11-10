using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Core.Data;
using Core.Misc;
using Core.Wpf.Mvvm;
using Prism.Commands;
using Prism.Mvvm;

namespace PatientInfoModule.ViewModels
{
    public class IdentityDocumentCollectionViewModel : BindableBase, IDisposable, IChangeTrackerMediator
    {
        private readonly Func<IdentityDocumentViewModel> identityDocumentFactory;

        public IdentityDocumentCollectionViewModel(Func<IdentityDocumentViewModel> identityDocumentFactory)
        {
            if (identityDocumentFactory == null)
            {
                throw new ArgumentNullException("identityDocumentFactory");
            }
            this.identityDocumentFactory = identityDocumentFactory;
            IdentityDocuments = new ObservableCollectionEx<IdentityDocumentViewModel>();
            IdentityDocuments.BeforeCollectionChanged += OnBeforeIdentityDocumentsCollectionChanged;
            ChangeTracker = new CompositeChangeTracker(new ObservableCollectionChangeTracker<IdentityDocumentViewModel>(IdentityDocuments));
            AddNewIdentityDocumentCommand = new DelegateCommand(AddNewIdentityDocument);
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
                }
            }
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.Cast<IdentityDocumentViewModel>())
                {
                    oldItem.DeleteRequested -= OnIdentityDocumentDeleteRequested;
                    oldItem.PropertyChanged -= OnIdentityDocumentPropertyChanged;
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
        
        public IEnumerable<PersonIdentityDocument> Model
        {
            get { return IdentityDocuments.Select(x => x.Model).ToArray(); }
            set
            {
                IdentityDocuments.Clear();
                foreach (var newModel in value)
                {
                    var newDocument = identityDocumentFactory();
                    newDocument.Model = newModel;
                    IdentityDocuments.Add(newDocument);
                }
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
        }

        private void OnIdentityDocumentDeleteRequested(object sender, EventArgs e)
        {
            IdentityDocuments.Remove(sender as IdentityDocumentViewModel);
        }

        public IChangeTracker ChangeTracker { get; private set; }

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
    }
}
