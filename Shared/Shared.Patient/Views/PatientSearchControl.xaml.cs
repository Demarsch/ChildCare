using Shared.Patient.ViewModels;

namespace Shared.Patient.Views
{
    /// <summary>
    /// Interaction logic for PatientSearchControl.xaml
    /// </summary>
    public partial class PatientSearchControl
    {
        public PatientSearchControl()
        {
            InitializeComponent();
        }

        public PersonSearchViewModel PersonSearchViewModel
        {
            get { return DataContext as PersonSearchViewModel; }
            set { DataContext = value; }
        }
    }
}
