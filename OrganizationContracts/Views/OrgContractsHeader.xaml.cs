using System;
using System.Collections.Generic;
using Fluent;
using Microsoft.Practices.Unity;
using OrganizationContractsModule.Misc;
using OrganizationContractsModule.ViewModels;

namespace OrganizationContractsModule.Views
{
    /// <summary>
    /// Interaction logic for OrgContractsHeader.xaml
    /// </summary>
    public partial class OrgContractsHeader
    {
        public OrgContractsHeader()
        {
            InitializeComponent();
        }

        [Dependency]
        public OrgContractsHeaderViewModel ViewModel
        {
            get { return DataContext as OrgContractsHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}
