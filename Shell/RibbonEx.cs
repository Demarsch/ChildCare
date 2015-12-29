using System.Windows;
using System.Windows.Controls;
using Fluent;
using ContextMenu = System.Windows.Controls.ContextMenu;

namespace Shell
{
    public class RibbonEx : Ribbon
    {
        public static readonly DependencyProperty ContextMenuOverrideProperty = DependencyProperty.Register("ContextMenuOverride", typeof (ContextMenu), typeof (RibbonEx), new PropertyMetadata(OnContextMenuChanged));

        private static void OnContextMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                ((ContextMenu)e.OldValue).PlacementTarget = null;
            }
            if (e.NewValue != null)
            {
                ((ContextMenu)e.NewValue).PlacementTarget = (UIElement)d;
            }
        }

        public ContextMenu ContextMenuOverride
        {
            get { return (ContextMenu)GetValue(ContextMenuOverrideProperty); }
            set { SetValue(ContextMenuOverrideProperty, value); }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            if (e.Source is IQuickAccessItemProvider)
            {
                base.OnContextMenuOpening(e);
                return;
            }
            var contextMenu = ContextMenuOverride;
            if (contextMenu != null)
            {
                contextMenu.IsOpen = true;
            }
            e.Handled = true;
        }
    }
}
