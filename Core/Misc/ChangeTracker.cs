using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Core.Annotations;

namespace Core.Misc
{
    public class ChangeTracker : INotifyPropertyChanged
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.Ordinal);

        private readonly Dictionary<string, object> comparers = new Dictionary<string, object>(StringComparer.Ordinal);

        private bool isEnabled;

        private bool hasChanges;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                values.Clear();
                HasChanges = false;
            }
        }

        public void UntrackAll()
        {
            values.Clear();
            HasChanges = false;
        }

        public void Track<TValue>(TValue oldValue, TValue newValue, [CallerMemberName] string propertyName = null)
        {
            if (!IsEnabled)
            {
                return;
            }
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName", "Property name can't be empty");
            }
            var comparer = GetComparer<TValue>(propertyName);
            if (comparer.Equals(oldValue, newValue))
            {
                return;
            }
            object originalValue;
            //If we have original value it means that we are already tracking this property
            if (values.TryGetValue(propertyName, out originalValue))
            {
                //If the new value is the same as original we untrack it
                if (comparer.Equals((TValue)originalValue, newValue))
                {
                    values.Remove(propertyName);
                }
                if (values.Count == 0)
                {
                    HasChanges = false;
                }
                //Otherwise it is still tracked
            }
            else
            {
                //Otherwise we start track it
                values[propertyName] = oldValue;
                if (values.Count == 1)
                {
                    HasChanges = true;
                }
            }
        }

        public void Untrack<TValue>(ref TValue originalValueStorage, Expression<Func<TValue>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }
            var propertyName = ((MemberExpression)propertyExpression.Body).Member.Name;
            object originalValue;
            if (values.TryGetValue(propertyName, out originalValue))
            {
                originalValueStorage = (TValue)originalValue;
                values.Remove(propertyName);
                HasChanges = values.Count > 0;
            }
        }

        public void RegisterComparer<TValue>(Expression<Func<TValue>> propertyExpression, IEqualityComparer<TValue> comparer)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            var propertyName = ((MemberExpression)propertyExpression.Body).Member.Name;
            comparers[propertyName] = comparer;
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

        public bool PropertyHasChanges<TValue>(Expression<Func<TValue>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }
            var propertyName = ((MemberExpression)propertyExpression.Body).Member.Name;
            return values.ContainsKey(propertyName);
        }

        private IEqualityComparer<TValue> GetComparer<TValue>(string propertyName)
        {
            object result;
            if (comparers.TryGetValue(propertyName, out result))
            {
                return result as IEqualityComparer<TValue>;
            }
            return EqualityComparer<TValue>.Default;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
