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
        public static readonly DependencyProperty DependentSelectorProperty = DependencyProperty.Register("DependentSelector", typeof(Selector), typeof(DependentCollectionNavigationBehavior), new PropertyMetadata(OnDependentItemsControlChanged));

        private static void OnDependentItemsControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (DependentCollectionNavigationBehavior)d;
            if (e.OldValue != null)
            {
                ((Selector)e.OldValue).IsSynchronizedWithCurrentItem = behavior.wasSynchronizedWithCurrentItem;
            }
            if (e.NewValue != null)
            {
                var selector = (Selector)e.NewValue;
                behavior.wasSynchronizedWithCurrentItem = selector.IsSynchronizedWithCurrentItem;
                selector.IsSynchronizedWithCurrentItem = true;
            }
        }

        private bool? wasSynchronizedWithCurrentItem;

        public ItemsControl DependentSelector
        {
            get { return (ItemsControl)GetValue(DependentSelectorProperty); }
            set { SetValue(DependentSelectorProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += AssociatedObjectOnPreviewKeyDown;
        }

        private void AssociatedObjectOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DependentSelector == null || DependentSelector.ItemsSource == null)
            {
                return;
            }
            var collectionView = CollectionViewSource.GetDefaultView(DependentSelector.ItemsSource);
            if (collectionView.IsEmpty)
            {
                return;
            }
            ContentControl itemContainer;
            switch (e.Key)
            {
                case Key.Down:
                    e.Handled = true;
                    if (collectionView.IsEmpty)
                    {
                        break;
                    }
                    if (!collectionView.MoveCurrentToNext())
                    {
                        collectionView.MoveCurrentToFirst();
                    }
                    itemContainer = (ContentControl)DependentSelector.ItemContainerGenerator.ContainerFromItem(collectionView.CurrentItem);
                    if (itemContainer != null)
                    {
                        itemContainer.BringIntoView();
                    }
                    break;
                case Key.Up:
                    e.Handled = true;
                    if (collectionView.IsEmpty)
                    {
                        break;
                    }
                    if (!collectionView.MoveCurrentToPrevious())
                    {
                        collectionView.MoveCurrentToLast();
                    }
                    itemContainer = (ContentControl)DependentSelector.ItemContainerGenerator.ContainerFromItem(collectionView.CurrentItem);
                    if (itemContainer != null)
                    {
                        itemContainer.BringIntoView();
                    }
                    break;
                case Key.Enter:
                    if (collectionView.IsEmpty)
                    {
                        return;
                    }
                    var currentItem = collectionView.CurrentItem ?? DependentSelector.ItemsSource.Cast<object>().First();
                    itemContainer = (ContentControl)DependentSelector.ItemContainerGenerator.ContainerFromItem(currentItem);
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
                            var events = new[] { UIElement.PreviewKeyDownEvent, UIElement.KeyDownEvent, UIElement.PreviewKeyUpEvent, UIElement.KeyUpEvent };
                            var keyEventArgs = new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, e.Key);
                            foreach (var @event in events)
                            {
                                keyEventArgs.RoutedEvent = @event;
                                itemContent.RaiseEvent(keyEventArgs);
                                if (keyEventArgs.Handled)
                                {
                                    break;
                                }
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
            DependentSelector = null;
            base.OnDetaching();
        }
    }
}
