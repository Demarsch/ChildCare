using Microsoft.Practices.Unity;
using ScheduleEditorModule.ViewModels;

namespace ScheduleEditorModule.Views
{
    /// <summary>
    /// Interaction logic for ContentView.xaml
    /// </summary>
    public partial class ContentView
    {
        public ContentView()
        {
            InitializeComponent();
        }

        [Dependency]
        public ContentViewModel ViewModel
        {
            get { return DataContext as ContentViewModel; }
            set { DataContext = value; }
        }

    }
}
