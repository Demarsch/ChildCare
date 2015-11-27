using Microsoft.Practices.Unity;
using ScheduleEditorModule.ViewModels;

namespace ScheduleEditorModule.Views
{
    /// <summary>
    /// Interaction logic for ScheduleEditorHeaderView.xaml
    /// </summary>
    public partial class ScheduleEditorHeaderView
    {
        public ScheduleEditorHeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public ScheduleEditorHeaderViewModel ViewModel
        {
            get { return DataContext as ScheduleEditorHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}
