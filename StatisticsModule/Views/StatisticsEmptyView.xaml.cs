using Microsoft.Practices.Unity;
using StatisticsModule.ViewModels;

namespace StatisticsModule.Views
{
    /// <summary>
    /// Interaction logic for StatisticsEmptyView.xaml
    /// </summary>
    public partial class StatisticsEmptyView
    {
        public StatisticsEmptyView()
        {
            InitializeComponent();
        }

        [Dependency]
        public StatisticsEmptyViewModel ViewModel
        {
            get { return DataContext as StatisticsEmptyViewModel; }
            set { DataContext = value; }
        }
    }
}
