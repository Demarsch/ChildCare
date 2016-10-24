using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using UserMessagerModule.ViewModels;
using Fluent;

namespace UserMessagerModule.Views
{
    /// <summary>
    /// Interaction logic for MessagerHeaderView.xaml
    /// </summary>
    public partial class MessagerHeaderView
    {
        public MessagerHeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public MessagerHeaderViewModel ViewModel
        {
            get { return DataContext as MessagerHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}