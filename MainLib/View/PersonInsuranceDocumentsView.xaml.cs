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
using System.Linq;

namespace MainLib
{
    /// <summary>
    /// Interaction logic for InsuranceDocementView.xaml
    /// </summary>
    public partial class PersonInsuranceDocumentsView : Window
    {
        public PersonInsuranceDocumentsView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox tbInShow = null;
            foreach (var item in Application.Current.Windows)
            {
                var uc = (item as Window).FindName("EditPersonDataUserControl") as UserControl;
                if (uc != null)
                    tbInShow = uc.FindName("tbInsurance") as TextBox;
                if (tbInShow != null)
                    break;
            }
            if (tbInShow != null)
            {
                var pos = tbInShow.PointToScreen(new Point(0, 0));
                this.Top = pos.Y;
                this.Left = pos.X;
            }
        }
    }
}
