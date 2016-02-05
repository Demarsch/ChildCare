using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using CommissionsModule.ViewModels;

namespace CommissionsModule.Views
{
    /// <summary>
    /// Interaction logic for CommissionsHeaderView.xaml
    /// </summary>
    public partial class CommissionProtocolHeaderView
    {
        public CommissionProtocolHeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public CommissionProtocolHeaderViewModel ViewModel
        {
            get { return DataContext as CommissionProtocolHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}