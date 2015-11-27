using Microsoft.Practices.Unity;
using ScheduleModule.ViewModels;

namespace ScheduleModule.Views
{
    /// <summary>
    /// Interaction logic for ScheduleContentView.xaml
    /// </summary>
    public partial class ScheduleContentView
    {
        public ScheduleContentView()
        {
            InitializeComponent();
        }

        [Dependency]
        public ScheduleContentViewModel ViewModel
        {
            get { return DataContext as ScheduleContentViewModel; }
            set { DataContext = value; }
        }
    }
}
