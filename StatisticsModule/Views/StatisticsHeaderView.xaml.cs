using Microsoft.Practices.Unity;
using StatisticsModule.ViewModels;

namespace StatisticsModule.Views
{
    /// <summary>
    /// Interaction logic for StatisticsHeaderView.xaml
    /// </summary>
    public partial class StatisticsHeaderView
    {
        public StatisticsHeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public StatisticsHeaderViewModel ViewModel
        {
            get { return DataContext as StatisticsHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}
