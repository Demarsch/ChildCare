using System.Collections;
using System.Windows.Input;

namespace Registry
{
    /// <summary>
    /// Interaction logic for PatientSearchUserControl.xaml
    /// </summary>
    public partial class PatientSearchUserControl
    {
        public PatientSearchUserControl()
        {
            InitializeComponent();
        }

        private void SearchPatientTextBoxOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                patientList.Focus();
            if (!patientList.HasItems)
                return;
            if (e.Key == Key.Down)
                SelectNextItem();
            else if (e.Key == Key.Up)
                SelectPreviousItem();
            else if (e.Key == Key.Enter)
                patientList.Focus();
        }

        private void SelectNextItem()
        {
            var source = patientList.ItemsSource as IList;
            if (source == null)
                return;
            if (patientList.SelectedIndex < source.Count - 1)
                patientList.SelectedIndex += 1;
            else
                patientList.SelectedIndex = 0;
        }

        private void SelectPreviousItem()
        {
            var source = patientList.ItemsSource as IList;
            if (source == null)
                return;
            if (patientList.SelectedIndex == -1)
                patientList.SelectedIndex = 0;
            else if (patientList.SelectedIndex == 0)
                patientList.SelectedIndex = source.Count - 1;
            else
                patientList.SelectedIndex -= 1;
        }
    }
}
