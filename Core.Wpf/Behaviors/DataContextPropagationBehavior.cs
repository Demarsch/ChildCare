using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Core.Wpf.Behaviors
{
    public class DataContextPropagationBehavior : Behavior<DataGrid>
    {
        static DataContextPropagationBehavior()
        {
            FrameworkElement.DataContextProperty.AddOwner(typeof(DataGridColumn));
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Columns.CollectionChanged += OnColumnsChanged;
            AssociatedObject.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            PropagateDataContextChanged();
        }

        private void OnColumnsChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            PropagateDataContextChanged();
        }

        private void PropagateDataContextChanged()
        {
            foreach (var column in AssociatedObject.Columns)
            {
                column.SetValue(FrameworkElement.DataContextProperty, AssociatedObject.DataContext);
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.DataContextChanged -= OnDataContextChanged;
            AssociatedObject.Columns.CollectionChanged -= OnColumnsChanged;
            base.OnDetaching();
        }
    }
}
