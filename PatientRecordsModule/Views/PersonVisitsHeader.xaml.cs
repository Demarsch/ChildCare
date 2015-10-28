using Microsoft.Practices.Unity;
using PatientRecordsModule.ViewModels;
using Prism;
using Prism.Mvvm;
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
    /// Interaction logic for PersonVisitsHeader.xaml
    /// </summary>
    public partial class PersonVisitsHeader
    {
        public PersonVisitsHeader()
        {
            InitializeComponent();
        }

        [Dependency]
        public PersonRecordsHeaderViewModel ViewModel
        {
            get { return DataContext as PersonRecordsHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}
