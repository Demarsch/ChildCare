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
using System.Windows.Shapes;

namespace AdminTools.View
{
    /// <summary>
    /// Interaction logic for UserAccountView.xaml
    /// </summary>
    public partial class UserAccountView : Window
    {
        public UserAccountView()
        {
            InitializeComponent();
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;

            if (item != null)
            {
                //item.IsSelected = true;
                //e.Handled = true;
            }
        }
        
    }
}
