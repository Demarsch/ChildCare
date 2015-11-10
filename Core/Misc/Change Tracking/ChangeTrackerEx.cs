using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Core.Annotations;
using Core.Extensions;

namespace Core.Misc
{
    public sealed class ChangeTrackerEx<TItem> : IChangeTracker where TItem : class, IChangeTrackable
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.Ordinal);

        private readonly Dictionary<string, object> comparers = new Dictionary<string, object>(StringComparer.Ordinal);

        private readonly TItem trackable;

        public ChangeTrackerEx(TItem trackable)
        {
            if (trackable == null)
            {
                throw new ArgumentNullException("trackable");
            }
            this.trackable = trackable;
            trackable.BeforeTrackedPropertyChanged += OnBeforeTrackablePropertyChanged;
            trackable.PropertyChanged += OnTrackablePropertyChanged;
        }

        private void OnTrackablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!IsEnabled || isRestoringChanges)
            {
                return;
            }
            var propertyName = e.PropertyName;
            if (string.IsNullOrEmpty(propertyName))
            {
                foreach (var trackedProperty in values.Keys.ToArray())
                {
                    CheckChanges(trackedProperty);
                }
            }
            else
            {
                CheckChanges(propertyName);
            }
        }

        private void CheckChanges(string propertyName)
        {
            object originalValue;
            //If we doesn't have original value that means we don't track this property
            if (!values.TryGetValue(propertyName, out originalValue))
            {
                return;
            }
            var newValue = trackable.GetValue(propertyName);
            var comparer = GetComparer(propertyName);
            //New value is the same as the original one - we remove original value
            if (comparer.Equals(originalValue, newValue))
            {
                values.Remove(propertyName);
            }
            HasChanges = values.Count > 0;
        }

        private void OnBeforeTrackablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!IsEnabled || isRestoringChanges)
            {
                return;
            }
            var propertyName = e.PropertyName;
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("Property name can't be empty");
            }
            object originalValue;
            //If we have original value then we shouldn't do anything
            if (values.TryGetValue(propertyName, out originalValue))
            {
                return;
            }
            values[propertyName] = sender.GetValue(propertyName);
        }

        private IEqualityComparer GetComparer(string propertyName)
        {
            object result;
            if (comparers.TryGetValue(propertyName, out result))
            {
                return result as IEqualityComparer;
            }
            return EqualityComparer<object>.Default;
        }

        private bool hasChanges;

        private bool isEnabled;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                values.Clear();
                HasChanges = false;
                if (value.Equals(isEnabled))
                {
                    return;
                }
                isEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool HasChanges
        {
            get { return hasChanges; }
            private set
            {
                if (value.Equals(hasChanges))
                {
                    return;
                }
                hasChanges = value;
                OnPropertyChanged();
            }
        }

        public bool PropertyHasChanges(string propertyName)
        {
            return values.ContainsKey(propertyName);
        }

        public void AcceptChanges()
        {
            if (!IsEnabled)
            {
                return;
            }
            values.Clear();
            HasChanges = false;
        }

        private bool isRestoringChanges;

        public void RestoreChanges()
        {
            if (!IsEnabled)
            {
                return;
            }
            isRestoringChanges = true;
            foreach (var trackedProperty in values)
            {
                trackable.SetValue(trackedProperty.Key, trackedProperty.Value);
            }
            values.Clear();
            HasChanges = false;
            isRestoringChanges = false;
        }

        public void RegisterComparer(string propertyName, IEqualityComparer comparer)
        {
            comparers[propertyName] = comparer;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Dispose()
        {
            trackable.BeforeTrackedPropertyChanged -= OnBeforeTrackablePropertyChanged;
            trackable.PropertyChanged -= OnTrackablePropertyChanged;
        }
    }
}
