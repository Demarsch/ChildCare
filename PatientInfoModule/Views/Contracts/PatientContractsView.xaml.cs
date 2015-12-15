using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;
using System.Windows.Input;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for PatientContractsView.xaml
    /// </summary>
    public partial class PatientContractsView
    {
        public PatientContractsView()
        {
            InitializeComponent();
        }

        [Dependency]
        public PatientContractsViewModel ViewModel
        {
            get { return DataContext as PatientContractsViewModel; }
            set { DataContext = value; }
        }

        private void autocomplete_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (autocomplete.SelectionAdapter != null)
            {
                if (autocomplete.IsDropDownOpen)
                    autocomplete.SelectionAdapter.HandleKeyDown(e);
                else
                    autocomplete.IsDropDownOpen = e.Key == Key.Down || e.Key == Key.Up;
            }
        }

        private void autocomplete_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            autocomplete.IsDropDownOpen = false;
        }
    }
}
