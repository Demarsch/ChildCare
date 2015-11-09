using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Core.Data;
using Core.Misc;
using Core.Wpf.Mvvm;
using Prism.Commands;
using Prism.Mvvm;

namespace PatientInfoModule.ViewModels
{
    public class IdentityDocumentCollectionViewModel : BindableBase, IDisposable
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
            ChangeTracker = new ObservableCollectionChangeTracker<IdentityDocumentViewModel>(IdentityDocuments);
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
            foreach (var newItem in e.NewItems.Cast<IdentityDocumentViewModel>())
            {
                newItem.DeleteRequested += OnIdentityDocumentDeleteRequested;
            }
            foreach (var oldItem in e.OldItems.Cast<IdentityDocumentViewModel>())
            {
                oldItem.DeleteRequested -= OnIdentityDocumentDeleteRequested;
            }
        }

        public ObservableCollectionEx<IdentityDocumentViewModel> IdentityDocuments { get; private set; }

        public IChangeTracker ChangeTracker { get; private set; }

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
            foreach (var identityDocument in IdentityDocuments)
            {
                identityDocument.DeleteRequested -= OnIdentityDocumentDeleteRequested;
            }
            IdentityDocuments.BeforeCollectionChanged -= OnBeforeIdentityDocumentsCollectionChanged;
        }

        private void OnIdentityDocumentDeleteRequested(object sender, EventArgs e)
        {
            IdentityDocuments.Remove(sender as IdentityDocumentViewModel);
        }
    }
}
