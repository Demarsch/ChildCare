using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using UserMessageModule.ViewModels;

namespace UserMessageModule.Views
{
    /// <summary>
    /// Interaction logic for MessageSelectorView.xaml
    /// </summary>
    public partial class MessageSelectorView
    {
        public MessageSelectorView()
        {
            InitializeComponent();
        }

        [Dependency]
        public MessageSelectorViewModel ViewModel
        {
            get { return DataContext as MessageSelectorViewModel; }
            set { DataContext = value; }
        }
    }
}