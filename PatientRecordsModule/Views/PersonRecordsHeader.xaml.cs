using Fluent;
using Microsoft.Practices.Unity;
using Shared.PatientRecords.ViewModels;
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

namespace Shared.PatientRecords.Views
{
    /// <summary>
    /// Interaction logic for PersonVisitsHeader.xaml
    /// </summary>
    public partial class PersonRecordsHeader
    {
        public PersonRecordsHeader()
        {
            InitializeComponent();
        }

        [Dependency]
        public PersonRecordsHeaderViewModel ViewModel
        {
            get { return DataContext as PersonRecordsHeaderViewModel; }
            set { DataContext = value; }
        }

        [Dependency(Shared.PatientRecords.Misc.Common.RibbonGroupName)]
        public RibbonContextualTabGroup ContextualTabGroup
        {
            set { Group = value; }
        }
    }
}
