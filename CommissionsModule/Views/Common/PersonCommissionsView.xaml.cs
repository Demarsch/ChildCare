using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using CommissionsModule.ViewModels.Common;
using System.Windows.Input;

namespace CommissionsModule.Views.Common
{
    /// <summary>
    /// Interaction logic for PersonCommissionsView.xaml
    /// </summary>
    public partial class PersonCommissionsView
    {
        public PersonCommissionsView()
        {
            InitializeComponent();
        }

        [Dependency]
        public PersonCommissionsViewModel ViewModel
        {
            get { return DataContext as PersonCommissionsViewModel; }
            set { DataContext = value; }
        }
    }

}
