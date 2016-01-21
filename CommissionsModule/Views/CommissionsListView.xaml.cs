using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using CommissionsModule.ViewModels;
using System.Windows.Input;

namespace CommissionsModule.Views
{
    /// <summary>
    /// Interaction logic for CommissionsListView.xaml
    /// </summary>
    public partial class CommissionsListView
    {
        public CommissionsListView()
        {
            InitializeComponent();
        }

        [Dependency]
        public CommissionsListViewModel ViewModel
        {
            get { return DataContext as CommissionsListViewModel; }
            set { DataContext = value; }
        }
    }
}