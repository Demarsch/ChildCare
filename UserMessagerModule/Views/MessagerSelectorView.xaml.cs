using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using UserMessagerModule.ViewModels;

namespace UserMessagerModule.Views
{
    /// <summary>
    /// Interaction logic for MessagerSelectorView.xaml
    /// </summary>
    public partial class MessagerSelectorView
    {
        public MessagerSelectorView()
        {
            InitializeComponent();
        }

        [Dependency]
        public MessagerSelectorViewModel ViewModel
        {
            get { return DataContext as MessagerSelectorViewModel; }
            set { DataContext = value; }
        }
    }
}