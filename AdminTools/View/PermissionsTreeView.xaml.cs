using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AdminTools.View
{
    /// <summary>
    /// Interaction logic for PermissionsTreeView.xaml
    /// </summary>
    public partial class PermissionsTreeView : Window
    {
        public PermissionsTreeView()
        {
            InitializeComponent();
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        private void PermissionTree_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeItem = (TreeViewItem)PermissionTree.ItemContainerGenerator.ContainerFromItem(PermissionTree.SelectedItem);
            if (treeItem != null)
                treeItem.IsSelected = false;            
        }
        
    }
}
