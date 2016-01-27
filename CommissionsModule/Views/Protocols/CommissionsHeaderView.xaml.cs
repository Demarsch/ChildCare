using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using CommissionsModule.ViewModels;

namespace CommissionsModule.Views
{
    /// <summary>
    /// Interaction logic for CommissionsHeaderView.xaml
    /// </summary>
    public partial class CommissionsHeaderView
    {
        public CommissionsHeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public CommissionsHeaderViewModel ViewModel
        {
            get { return DataContext as CommissionsHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}