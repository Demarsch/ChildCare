/*using Microsoft.Practices.Unity;
using Shared.Commissions.ViewModels;
namespace Shared.Commissions.Views
{
    /// <summary>
    /// Interaction logic for AssignmentCommissionView.xaml
    /// </summary>
    public partial class AssignmentCommissionView
    {
        public AssignmentCommissionView()
        {
            InitializeComponent();
        }

        [Dependency]
        public AssignmentCommissionViewModel ViewModel
        {
            get { return DataContext as AssignmentCommissionViewModel; }
            set { DataContext = value; }
        }
    }
}*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Shared.Commissions.Views
{
    /// <summary>
    /// Interaction logic for AssignmentCommissionView.xaml
    /// </summary>
    public partial class AssignmentCommissionView : UserControl
    {
        public AssignmentCommissionView()
        {
            InitializeComponent();
        }
    }
}