using Microsoft.Practices.Unity;
using Shared.Patient.ViewModels;
namespace Shared.Patient.Views
{
    /// <summary>
    /// Interaction logic for AddressView.xaml
    /// </summary>
    public partial class AddressView
    {
        public AddressView()
        {
            InitializeComponent();
        }

        [Dependency]
        public AddressViewModel ViewModel
        {
            get { return DataContext as AddressViewModel; }
            set { DataContext = value; }
        }
    }
}
