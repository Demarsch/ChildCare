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
using Shared.Patient.ViewModels;

namespace PatientInfoModule.ViewModels
{
    public class AddressCollectionViewModel : BindableBase, IDisposable, IChangeTrackerMediator, IActiveDataErrorInfo
    {
        private readonly Func<AddressViewModel> addressFactory;

        private readonly CompositeChangeTracker changeTracker;

        public AddressCollectionViewModel(Func<AddressViewModel> addressFactory)
        {
            if (addressFactory == null)
            {
                throw new ArgumentNullException("addressFactory");
            }
            this.addressFactory = addressFactory;
            Addresses = new ObservableCollectionEx<AddressViewModel>();
            Addresses.BeforeCollectionChanged += OnBeforeAddressesCollectionChanged;
            Addresses.CollectionChanged += OnAddressesCollectionChanged;
            changeTracker = new CompositeChangeTracker(new ObservableCollectionChangeTracker<AddressViewModel>(Addresses));
            AddNewAddressCommand = new DelegateCommand(AddNewAddress);
        }

        private void OnAddressesCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            OnPropertyChanged(() => StringRepresentation);
        }

        public ICommand AddNewAddressCommand { get; private set; }

        private void AddNewAddress()
        {
            var newAddress = addressFactory();
            Addresses.Add(newAddress);
        }

        private void OnBeforeAddressesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<AddressViewModel>())
                {
                    newItem.DeleteRequested += OnAddressDeleteRequested;
                    newItem.PropertyChanged += OnAddressPropertyChanged;
                    changeTracker.AddTracker(newItem.CompositeChangeTracker);
                }
            }
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.Cast<AddressViewModel>())
                {
                    oldItem.DeleteRequested -= OnAddressDeleteRequested;
                    oldItem.PropertyChanged -= OnAddressPropertyChanged;
                    changeTracker.RemoveTracker(oldItem.CompositeChangeTracker);
                }
            }
        }

        private void OnAddressPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (string.IsNullOrEmpty(propertyChangedEventArgs.PropertyName) || string.CompareOrdinal(propertyChangedEventArgs.PropertyName, "StringRepresentation") == 0)
            {
                OnPropertyChanged(() => StringRepresentation);
            }
        }

        public ObservableCollectionEx<AddressViewModel> Addresses { get; private set; }
        
        public ICollection<PersonAddress> Model
        {
            get { return Addresses.Select(x => x.Model).ToArray(); }
            set
            {
                value = value ?? new PersonAddress[0];
                CompositeChangeTracker.IsEnabled = false;
                Addresses.Clear();
                foreach (var newModel in value)
                {
                    var newDocument = addressFactory();
                    newDocument.Model = newModel;
                    Addresses.Add(newDocument);
                }
                CompositeChangeTracker.IsEnabled = true;
            }
        }

        public void Dispose()
        {
            CompositeChangeTracker.Dispose();
            foreach (var address in Addresses)
            {
                address.DeleteRequested -= OnAddressDeleteRequested;
                address.PropertyChanged -= OnAddressPropertyChanged;
            }
            Addresses.BeforeCollectionChanged -= OnBeforeAddressesCollectionChanged;
            Addresses.CollectionChanged -= OnAddressesCollectionChanged;
        }

        private void OnAddressDeleteRequested(object sender, EventArgs e)
        {
            Addresses.Remove(sender as AddressViewModel);
        }

        public IChangeTracker CompositeChangeTracker
        {
            get { return changeTracker; }
        }

        public string StringRepresentation
        {
            get
            {
                var documentsRepresentations = Addresses.Select(x => x.StringRepresentation)
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
                    return Addresses.Select(x => x.Error).FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? string.Empty;
                }
                return string.Empty;
            }
        }

        public string Error { get { throw new NotImplementedException(); } }

        public bool Validate()
        {
            var result = Addresses.Select(x => x.Validate()).ToArray();
            return result.All(x => x);
        }

        public void CancelValidation()
        {
            Addresses.ForEach(x => x.CancelValidation());
        }
    }
}
