using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using CommissionsModule.ViewModels;
using Fluent;

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

        private void ToggleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if ((sender as ToggleButton).IsChecked != true)
                (sender as ToggleButton).IsChecked = true;
        }
    }
}