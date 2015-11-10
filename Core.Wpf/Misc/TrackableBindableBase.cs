using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.Misc;
using Prism.Mvvm;

namespace Core.Wpf.Misc
{
    public class TrackableBindableBase : BindableBase, IChangeTrackable
    {
        public event PropertyChangedEventHandler BeforeTrackedPropertyChanged;

        protected virtual void OnBeforeTrackedPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = BeforeTrackedPropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public bool SetTrackedProperty<TItem>(ref TItem storage, TItem value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<TItem>.Default.Equals(storage, value))
            {
                return false;
            }
            OnBeforeTrackedPropertyChanged(new PropertyChangedEventArgs(propertyName));
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
