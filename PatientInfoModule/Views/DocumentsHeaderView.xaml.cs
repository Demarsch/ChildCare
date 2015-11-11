using Fluent;
using Microsoft.Practices.Unity;
using PatientInfoModule.Misc;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for InfoHeader.xaml
    /// </summary>
    public partial class DocumentsHeaderView
    {
        public DocumentsHeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public DocumentsHeaderViewModel InfoHeaderViewModel
        {
            get { return DataContext as DocumentsHeaderViewModel; }
            set { DataContext = value; }
        }

        [Dependency(Common.RibbonGroupName)]
        public RibbonContextualTabGroup ContextualTabGroup
        {
            set { Group = value; }
        }
    }
}
