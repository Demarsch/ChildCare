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
    public class SocialStatusCollectionViewModel : BindableBase, IDisposable, IChangeTrackerMediator, IActiveDataErrorInfo
    {
        private readonly Func<SocialStatusViewModel> socialStatusFactory;

        private readonly CompositeChangeTracker changeTracker;

        public SocialStatusCollectionViewModel(Func<SocialStatusViewModel> socialStatusFactory)
        {
            if (socialStatusFactory == null)
            {
                throw new ArgumentNullException("socialStatusFactory");
            }
            this.socialStatusFactory = socialStatusFactory;
            SocialStatuses = new ObservableCollectionEx<SocialStatusViewModel>();
            SocialStatuses.BeforeCollectionChanged += OnBeforeSocialStatusesCollectionChanged;
            SocialStatuses.CollectionChanged += OnSocialStatusesCollectionChanged;
            changeTracker = new CompositeChangeTracker(new ObservableCollectionChangeTracker<SocialStatusViewModel>(SocialStatuses));
            AddNewSocialStatusCommand = new DelegateCommand(AddNewSocialStatus);
        }

        private void OnSocialStatusesCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            OnPropertyChanged(() => StringRepresentation);
        }

        public ICommand AddNewSocialStatusCommand { get; private set; }

        private void AddNewSocialStatus()
        {
            var newSocialStatus = socialStatusFactory();
            SocialStatuses.Add(newSocialStatus);
        }

        private void OnBeforeSocialStatusesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<SocialStatusViewModel>())
                {
                    newItem.DeleteRequested += OnSocialStatusDeleteRequested;
                    newItem.PropertyChanged += OnSocialStatusPropertyChanged;
                    changeTracker.AddTracker(newItem.ChangeTracker);
                }
            }
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.Cast<SocialStatusViewModel>())
                {
                    oldItem.DeleteRequested -= OnSocialStatusDeleteRequested;
                    oldItem.PropertyChanged -= OnSocialStatusPropertyChanged;
                    changeTracker.RemoveTracker(oldItem.ChangeTracker);
                }
            }
        }

        private void OnSocialStatusPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (string.IsNullOrEmpty(propertyChangedEventArgs.PropertyName) || string.CompareOrdinal(propertyChangedEventArgs.PropertyName, "StringRepresentation") == 0)
            {
                OnPropertyChanged(() => StringRepresentation);
            }
        }

        public ObservableCollectionEx<SocialStatusViewModel> SocialStatuses { get; private set; }
        
        public ICollection<PersonSocialStatus> Model
        {
            get { return SocialStatuses.Select(x => x.Model).ToArray(); }
            set
            {
                value = value ?? new PersonSocialStatus[0];
                ChangeTracker.IsEnabled = false;
                SocialStatuses.Clear();
                foreach (var newModel in value)
                {
                    var newSocialStatus = socialStatusFactory();
                    newSocialStatus.Model = newModel;
                    SocialStatuses.Add(newSocialStatus);
                }
                ChangeTracker.IsEnabled = true;
            }
        }

        public void Dispose()
        {
            ChangeTracker.Dispose();
            foreach (var socialStatus in SocialStatuses)
            {
                socialStatus.DeleteRequested -= OnSocialStatusDeleteRequested;
                socialStatus.PropertyChanged -= OnSocialStatusPropertyChanged;
            }
            SocialStatuses.BeforeCollectionChanged -= OnBeforeSocialStatusesCollectionChanged;
            SocialStatuses.CollectionChanged -= OnSocialStatusesCollectionChanged;
        }

        private void OnSocialStatusDeleteRequested(object sender, EventArgs e)
        {
            SocialStatuses.Remove(sender as SocialStatusViewModel);
        }

        public IChangeTracker ChangeTracker
        {
            get { return changeTracker; }
        }

        public string StringRepresentation
        {
            get
            {
                var documentsRepresentations = SocialStatuses.Select(x => x.StringRepresentation)
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
                    return SocialStatuses.Select(x => x.Error).FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? string.Empty;
                }
                return string.Empty;
            }
        }

        public string Error { get { throw new NotImplementedException(); } }

        public bool Validate()
        {
            var result = SocialStatuses.Select(x => x.Validate()).ToArray();
            return result.All(x => x);
        }

        public void CancelValidation()
        {
            SocialStatuses.ForEach(x => x.CancelValidation());
        }
    }
}
