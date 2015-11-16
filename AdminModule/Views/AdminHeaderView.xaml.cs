using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using AdminModule.ViewModels;

namespace AdminModule.Views
{
    /// <summary>
    /// Interaction logic for AdminHeaderView.xaml
    /// </summary>
    public partial class AdminHeaderView
    {
        public AdminHeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public AdminHeaderViewModel ViewModel
        {
            get { return DataContext as AdminHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}
