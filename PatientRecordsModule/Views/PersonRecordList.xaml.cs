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
    public partial class PersonRecordList
    {
        public PersonRecordList()
        {
            InitializeComponent();
        }

        [Dependency]
        public PersonRecordListViewModel ContentViewModel
        {
            get { return DataContext as PersonRecordListViewModel; }
            set { DataContext = value; }
        }
    }
}
