using CommissionsModule.ViewModels;
using Microsoft.Practices.Unity;
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

namespace CommissionsModule.Views
{
    /// <summary>
    /// Interaction logic for CommissionJournalHeaderView.xaml
    /// </summary>
    public partial class CommissionJournalHeaderView
    {
        public CommissionJournalHeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public CommissionJournalHeaderViewModel ViewModel
        {
            get { return DataContext as CommissionJournalHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}
