using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Core.Misc
{
    public abstract class ValidationMediator<TItem> : IActiveDataErrorInfo where TItem : INotifyPropertyChanged
    {
        protected readonly TItem AssociatedItem;

        private readonly Dictionary<string, string> Errors = new Dictionary<string, string>(StringComparer.Ordinal);

        private bool ValidationIsActive { get; set; }

        protected ValidationMediator(TItem associatedItem)
        {
            AssociatedItem = associatedItem;
            AssociatedItem.PropertyChanged += ParentViewModelOnPropertyChanged;
        }

        protected bool PropertyNameEquals<TProperty>(string propertyName, Expression<Func<TItem, TProperty>> propertyExpression)
        {
            return string.CompareOrdinal(propertyName, GetPropertyName(propertyExpression)) == 0;
        }

        private string GetPropertyName<TProperty>(Expression<Func<TItem, TProperty>> propertyExpression)
        {
            return ((MemberExpression)propertyExpression.Body).Member.Name;
        }

        protected void SetError<TProperty>(Expression<Func<TItem, TProperty>> propertyExpression, string error)
        {
            if (!string.IsNullOrEmpty(error) || ValidationIsActive)
            {
                Errors[GetPropertyName(propertyExpression)] = error;
            }
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