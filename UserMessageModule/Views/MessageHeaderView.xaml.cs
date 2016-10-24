using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using UserMessageModule.ViewModels;
using Fluent;

namespace UserMessageModule.Views
{
    /// <summary>
    /// Interaction logic for MessageHeaderView.xaml
    /// </summary>
    public partial class MessageHeaderView
    {
        public MessageHeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public MessageHeaderViewModel ViewModel
        {
            get { return DataContext as MessageHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}