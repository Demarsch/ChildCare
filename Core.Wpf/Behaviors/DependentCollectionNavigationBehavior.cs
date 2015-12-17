using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using Xceed.Wpf.AvalonDock.Controls;

namespace Core.Wpf.Behaviors
{
    public class DependentCollectionNavigationBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty DependentItemsControlProperty = DependencyProperty.Register("DependentItemsControl", typeof(ItemsControl), typeof(DependentCollectionNavigationBehavior), new PropertyMetadata(null));

        public ItemsControl DependentItemsControl
        {
            get { return (ItemsControl)GetValue(DependentItemsControlProperty); }
            set { SetValue(DependentItemsControlProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += AssociatedObjectOnPreviewKeyDown;
        }

        private void AssociatedObjectOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DependentItemsControl == null || DependentItemsControl.ItemsSource == null)
            {
                return;
            }
            var collectionView = CollectionViewSource.GetDefaultView(DependentItemsControl.ItemsSource);
            if (collectionView.IsEmpty)
            {
                return;
            }
            switch (e.Key)
            {
                case Key.Down:
                    if (!collectionView.MoveCurrentToNext())
                    {
                        collectionView.MoveCurrentToFirst();
                    }
                    e.Handled = true;
                    break;
                case Key.Up:
                   if (!collectionView.MoveCurrentToPrevious())
                    {
                        collectionView.MoveCurrentToLast();
                    }
                    e.Handled = true;
                    break;
                case Key.Enter:
                    var currentItem = collectionView.CurrentItem;
                    if (currentItem != null)
                    {
                        var itemContainer = (ContentControl)DependentItemsControl.ItemContainerGenerator.ContainerFromItem(currentItem);
                        if (itemContainer == null)
                        {
                            return;
                        }
                        var contentPresenter = itemContainer.FindVisualChildren<ContentPresenter>().FirstOrDefault();
                        if (contentPresenter == null)
                        {
                            return;
                        }
                        var itemContent = (UIElement)VisualTreeHelper.GetChild(contentPresenter, 0);
                        if (itemContent != null)
                        {
                            var commandSource = itemContent as ICommandSource;
                            if (commandSource != null && commandSource.Command != null && commandSource.Command.CanExecute(commandSource.CommandParameter))
                            {
                                commandSource.Command.Execute(commandSource.CommandParameter);
                            }
                            else
                            {
                                itemContent.RaiseEvent(new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, e.Key) { RoutedEvent = Keyboard.PreviewKeyDownEvent });                                
                            }
                        }
                    }
                    e.Handled = true;
                    break;
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= AssociatedObjectOnPreviewKeyDown;
            DependentItemsControl = null;
            base.OnDetaching();
        }
    }
}
