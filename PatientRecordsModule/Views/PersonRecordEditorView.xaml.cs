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
    /// Interaction logic for PersonRecordEditorView.xaml
    /// </summary>
    public partial class PersonRecordEditorView
    {
        public PersonRecordEditorView()
        {
            InitializeComponent();
        }

        [Dependency]
        public PersonRecordEditorViewModel ContentViewModel
        {
            get { return DataContext as PersonRecordEditorViewModel; }
            set { DataContext = value; }
        }

        private void ListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            (sender as ListBox).SelectedItem = null;
        }
    }
}
