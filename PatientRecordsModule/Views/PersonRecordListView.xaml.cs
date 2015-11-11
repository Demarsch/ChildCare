using Microsoft.Practices.Unity;
using PatientRecordsModule.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PatientRecordsModule.Views
{
    /// <summary>
    /// Interaction logic for PersonRecordListView.xaml
    /// </summary>
    public partial class PersonRecordListView
    {
        public PersonRecordListView()
        {
            InitializeComponent();
        }

        [Dependency]
        public PersonRecordListViewModel ContentViewModel
        {
            get { return DataContext as PersonRecordListViewModel; }
            set { DataContext = value; }
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
            TreeViewItem treeItem = (TreeViewItem)itemsTree.ItemContainerGenerator.ContainerFromItem(itemsTree.SelectedItem);
            if (treeItem != null)
                treeItem.IsSelected = false;
        }
    }
}
