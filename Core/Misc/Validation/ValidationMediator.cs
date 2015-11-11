using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Core.Misc
{
    public abstract class ValidationMediator<TItem> : IActiveDataErrorInfo where TItem : INotifyPropertyChanged
    {
        protected readonly TItem AssociatedItem;

        protected readonly Dictionary<string, string> Errors = new Dictionary<string, string>(StringComparer.Ordinal);

        protected bool ValidationIsActive { get; private set; }

        protected ValidationMediator(TItem associatedItem)
        {
            AssociatedItem = associatedItem;
            AssociatedItem.PropertyChanged += ParentViewModelOnPropertyChanged;
        }

        private void ParentViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ValidationIsActive)
            {
                if (string.IsNullOrEmpty(e.PropertyName))
                {
                    OnValidate();
                }
                else
                {
                    OnValidateProperty(e.PropertyName);
                }
            }
        }

        protected abstract void OnValidateProperty(string propertyName);

        protected abstract void RaiseAssociatedObjectPropertyChanged();

        public bool Validate()
        {
            ValidationIsActive = true;
            Errors.Clear();
            OnValidate();
            RaiseAssociatedObjectPropertyChanged();
            return Errors.Count(x => !string.IsNullOrEmpty(x.Value)) == 0;
        }

        protected abstract void OnValidate();

        public void CancelValidation()
        {
            ValidationIsActive = false;
            Errors.Clear();
            RaiseAssociatedObjectPropertyChanged();
        }

        public string this[string columnName]
        {
            get
            {
                string result;
                return Errors.TryGetValue(columnName, out result) ? result : string.Empty;
            }
        }

        public string Error
        {
            get
            {
                return string.Join(Environment.NewLine, Errors.Where(x => !string.IsNullOrEmpty(x.Value))
                                                              .Select(x => x.Value)
                                                              .Distinct(StringComparer.CurrentCulture));
            }
        }
    }
}