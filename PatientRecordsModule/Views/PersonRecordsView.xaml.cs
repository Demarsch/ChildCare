using Microsoft.Practices.Unity;
using Shared.PatientRecords.ViewModels;
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
    /// Interaction logic for PersonRecordsView.xaml
    /// </summary>
    public partial class PersonRecordsView
    {
        public PersonRecordsView()
        {
            InitializeComponent();
        }

        [Dependency]
        public PersonRecordsViewModel ContentViewModel
        {
            get { return DataContext as PersonRecordsViewModel; }
            set { DataContext = value; }
        }

        [Dependency]
        public PersonRecordListViewModel PersonVisitItemsListViewModel
        {
            get { return personRecordList.DataContext as PersonRecordListViewModel; }
            set { personRecordList.DataContext = value; }
        }

        [Dependency]
        public PersonRecordEditorViewModel PersonRecordEditorViewModel
        {
            get { return personRecordEditor.DataContext as PersonRecordEditorViewModel; }
            set { personRecordEditor.DataContext = value; }
        }
    }
}
