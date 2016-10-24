using UserMessageModule.ViewModels;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UserMessageModule.Views
{
    /// <summary>
    /// Interaction logic for MessageInboxView.xaml
    /// </summary>
    public partial class MessageInboxView : UserControl
    {
        public MessageInboxView()
        {
            InitializeComponent();
        }

        [Dependency]
        public MessageInboxViewModel ViewModel
        {
            get { return DataContext as MessageInboxViewModel; }
            set { DataContext = value; }
        }
    }
}
