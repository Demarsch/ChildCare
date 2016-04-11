using StatisticsModule.ViewModels;
using Microsoft.Practices.Unity;

namespace StatisticsModule.Views
{
    /// <summary>
    /// Interaction logic for RecordsStatisticsView.xaml
    /// </summary>
    public partial class RecordsStatisticsView
    {
        public RecordsStatisticsView()
        {
            InitializeComponent();
        }

        [Dependency]
        public RecordsStatisticsViewModel RecordsStatisticsViewModel
        {
            get { return DataContext as RecordsStatisticsViewModel; }
            set { DataContext = value; }
        }
    }
}