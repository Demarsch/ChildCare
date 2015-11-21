using Microsoft.Practices.Unity;
using ScheduleEditorModule.ViewModels;

namespace ScheduleEditorModule.Views
{
    /// <summary>
    /// Interaction logic for HeaderView.xaml
    /// </summary>
    public partial class HeaderView
    {
        public HeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public HeaderViewModel ViewModel
        {
            get { return DataContext as HeaderViewModel; }
            set { DataContext = value; }
        }
    }
}
