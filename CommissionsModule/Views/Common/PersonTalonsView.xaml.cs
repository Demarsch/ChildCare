using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using System.Windows.Input;
using CommissionsModule.ViewModels.Common;

namespace CommissionsModule.Views.Common
{
    /// <summary>
    /// Interaction logic for PersonTalonsView.xaml
    /// </summary>
    public partial class PersonTalonsView
    {
        public PersonTalonsView()
        {
            InitializeComponent();
        }

        [Dependency]
        public PersonTalonsViewModel ViewModel
        {
            get { return DataContext as PersonTalonsViewModel; }
            set { DataContext = value; }
        }
    }

}