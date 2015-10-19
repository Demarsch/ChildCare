using Microsoft.Practices.Unity;

namespace Shell
{
    /// <summary>
    /// Interaction logic for ShellWindow.xaml
    /// </summary>
    public partial class ShellWindow
    {
        public ShellWindow()
        {
            InitializeComponent();
        }

        [Dependency]
        public ShellWindowViewModel ShellWindowViewModel
        {
            get { return DataContext as ShellWindowViewModel; }
            set { DataContext = value; }
        }
    }
}
