using Microsoft.Practices.Unity;
using ScheduleEditorModule.ViewModels;

namespace ScheduleEditorModule.Views
{
    /// <summary>
    /// Interaction logic for ScheduleEditorContentView.xaml
    /// </summary>
    public partial class ScheduleEditorContentView
    {
        public ScheduleEditorContentView()
        {
            InitializeComponent();
        }

        [Dependency]
        public ScheduleEditorContentViewModel ViewModel
        {
            get { return DataContext as ScheduleEditorContentViewModel; }
            set { DataContext = value; }
        }

    }
}
