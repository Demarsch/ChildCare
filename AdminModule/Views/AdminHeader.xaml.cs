using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using AdminModule.ViewModels;

namespace AdminModule.Views
{
    /// <summary>
    /// Interaction logic for AdminHeader.xaml
    /// </summary>
    public partial class AdminHeader
    {
        public AdminHeader()
        {
            InitializeComponent();
        }

        [Dependency]
        public AdminViewModel ViewModel
        {
            get { return DataContext as AdminViewModel; }
            set { DataContext = value; }
        }
    }
}
