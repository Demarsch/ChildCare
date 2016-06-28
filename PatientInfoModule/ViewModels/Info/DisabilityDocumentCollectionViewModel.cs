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
    public class DisabilityDocumentCollectionViewModel : BindableBase, IDisposable, IChangeTrackerMediator, IActiveDataErrorInfo
    {
        private readonly Func<DisabilityDocumentViewModel> disabilityDocumentFactory;

        private readonly CompositeChangeTracker changeTracker;

        public DisabilityDocumentCollectionViewModel(Func<DisabilityDocumentViewModel> disabilityDocumentFactory)
        {
            if (disabilityDocumentFactory == null)
            {
                throw new ArgumentNullException("disabilityDocumentFactory");
            }
            this.disabilityDocumentFactory = disabilityDocumentFactory;
            DisabilityDocuments = new ObservableCollectionEx<DisabilityDocumentViewModel>();
            DisabilityDocuments.BeforeCollectionChanged += OnBeforeDisabilityDocumentsCollectionChanged;
            DisabilityDocuments.CollectionChanged += OnDisabilityDocumentsCollectionChanged;
            changeTracker = new CompositeChangeTracker(new ObservableCollectionChangeTracker<DisabilityDocumentViewModel>(DisabilityDocuments));
            AddNewDisabilityDocumentCommand = new DelegateCommand(AddNewIdentityDocument);
        }

        private void OnDisabilityDocumentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            OnPropertyChanged(() => StringRepresentation);
        }

        public ICommand AddNewDisabilityDocumentCommand { get; private set; }

        private void AddNewIdentityDocument()
        {
            var newDisabilityDocument = disabilityDocumentFactory();
            DisabilityDocuments.Add(newDisabilityDocument);
        }

        private void OnBeforeDisabilityDocumentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<DisabilityDocumentViewModel>())
                {
                    newItem.DeleteRequested += OnDisabilityDocumentDeleteRequested;
                    newItem.PropertyChanged += OnDisabilityDocumentPropertyChanged;
                    changeTracker.AddTracker(newItem.ChangeTracker);
                }
            }
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.Cast<DisabilityDocumentViewModel>())
                {
                    oldItem.DeleteRequested -= OnDisabilityDocumentDeleteRequested;
                    oldItem.PropertyChanged -= OnDisabilityDocumentPropertyChanged;
                    changeTracker.RemoveTracker(oldItem.ChangeTracker);
                }
            }
        }

        private void OnDisabilityDocumentPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (string.IsNullOrEmpty(propertyChangedEventArgs.PropertyName) || string.CompareOrdinal(propertyChangedEventArgs.PropertyName, "StringRepresentation") == 0)
            {
                OnPropertyChanged(() => StringRepresentation);
            }
        }

        public ObservableCollectionEx<DisabilityDocumentViewModel> DisabilityDocuments { get; private set; }
        
        public ICollection<PersonDisability> Model
        {
            get { return DisabilityDocuments.Select(x => x.Model).ToArray(); }
            set
            {
                value = value ?? new PersonDisability[0];
                ChangeTracker.IsEnabled = false;
                DisabilityDocuments.Clear();
                foreach (var newModel in value)
                {
                    var newDocument = disabilityDocumentFactory();
                    newDocument.Model = newModel;
                    DisabilityDocuments.Add(newDocument);
                }
                ChangeTracker.IsEnabled = true;
            }
        }

        public void Dispose()
        {
            ChangeTracker.Dispose();
            foreach (var disabilityDocument in DisabilityDocuments)
            {
                disabilityDocument.DeleteRequested -= OnDisabilityDocumentDeleteRequested;
                disabilityDocument.PropertyChanged -= OnDisabilityDocumentPropertyChanged;
            }
            DisabilityDocuments.BeforeCollectionChanged -= OnBeforeDisabilityDocumentsCollectionChanged;
            DisabilityDocuments.CollectionChanged -= OnDisabilityDocumentsCollectionChanged;
        }

        private void OnDisabilityDocumentDeleteRequested(object sender, EventArgs e)
        {
            DisabilityDocuments.Remove(sender as DisabilityDocumentViewModel);
        }

        public IChangeTracker ChangeTracker
        {
            get { return changeTracker; }
        }

        public string StringRepresentation
        {
            get
            {
                var documentsRepresentations = DisabilityDocuments.Select(x => x.StringRepresentation)
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
                    return DisabilityDocuments.Select(x => x.Error).FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? string.Empty;
                }
                return string.Empty;
            }
        }

        public string Error { get { throw new NotImplementedException(); } }

        public bool Validate()
        {
            var result = DisabilityDocuments.Select(x => x.Validate()).ToArray();
            return result.All(x => x);
        }

        public void CancelValidation()
        {
            DisabilityDocuments.ForEach(x => x.CancelValidation());
        }
    }
}
