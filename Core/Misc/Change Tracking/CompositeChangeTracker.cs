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
    public sealed class CompositeChangeTracker : IChangeTracker
    {
        private readonly HashSet<IChangeTracker> trackers;

        public CompositeChangeTracker()
        {
            trackers = new HashSet<IChangeTracker>();
        }

        public CompositeChangeTracker(params IChangeTracker[] trackers)
            : this()
        {
            if (trackers == null)
            {
                throw new ArgumentNullException("trackers");
            }
            foreach (var tracker in trackers)
            {
                AddTracker(tracker);
            }
        }

        public void AddTracker(IChangeTracker tracker)
        {
            if (trackers.Add(tracker))
            {
                tracker.PropertyChanged += OnTrackerPropertyChanged;
            }
        }

        public void RemoveTracker(IChangeTracker tracker)
        {
            if (trackers.Remove(tracker))
            {
                tracker.PropertyChanged -= OnTrackerPropertyChanged;
            }
        }

        private void OnTrackerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (currentTrackerIsSourceOfChanges)
            {
                return;
            }
            OnPropertyChanged(e.PropertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            trackers.ForEach(x => x.Dispose());
        }

        public bool IsEnabled
        {
            get { return trackers.Count > 0 && trackers.All(x => x.IsEnabled); }
            set
            {
                currentTrackerIsSourceOfChanges = true;
                trackers.ForEach(x => x.IsEnabled = value);
                currentTrackerIsSourceOfChanges = false;
            }
        }

        public bool HasChanges
        {
            get { return trackers.Any(x => x.HasChanges); }
        }

        public bool PropertyHasChanges(string propertyName)
        {
            return trackers.Any(x => x.PropertyHasChanges(propertyName));
        }

        public void AcceptChanges()
        {
            if (!IsEnabled)
            {
                return;
            }
            currentTrackerIsSourceOfChanges = true;
            trackers.ForEach(x => x.AcceptChanges());
            currentTrackerIsSourceOfChanges = false;
            OnPropertyChanged("HasChanges");
        }

        private bool currentTrackerIsSourceOfChanges;

        public void RestoreChanges()
        {
            if (!IsEnabled)
            {
                return;
            }
            currentTrackerIsSourceOfChanges = true;
            trackers.ToArray().ForEach(x => x.RestoreChanges());
            currentTrackerIsSourceOfChanges = false;
            OnPropertyChanged("HasChanges");
        }

        public void RegisterComparer(string propertyName, IEqualityComparer comparer)
        {
            trackers.ForEach(x => x.RegisterComparer(propertyName, comparer));
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
