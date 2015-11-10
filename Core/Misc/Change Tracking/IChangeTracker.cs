using System;
using System.Collections;
using System.ComponentModel;

namespace Core.Misc
{
    public interface IChangeTracker : INotifyPropertyChanged, IDisposable
    {
        bool IsEnabled { get; set; }

        bool HasChanges { get; }

        bool PropertyHasChanges(string propertyName);

        void AcceptChanges();

        void RestoreChanges();

        void RegisterComparer(string propertyName, IEqualityComparer comparer);
    }
}
