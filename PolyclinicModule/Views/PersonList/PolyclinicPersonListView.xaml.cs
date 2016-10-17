using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using PolyclinicModule.ViewModels;
using System.Windows.Input;

namespace PolyclinicModule.Views
{
    /// <summary>
    /// Interaction logic for PolyclinicPersonListView.xaml
    /// </summary>
    public partial class PolyclinicPersonListView
    {
        public PolyclinicPersonListView()
        {
            InitializeComponent();
        }

        [Dependency]
        public PolyclinicPersonListViewModel ViewModel
        {
            get { return DataContext as PolyclinicPersonListViewModel; }
            set { DataContext = value; }
        }
    }
}