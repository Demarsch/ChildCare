using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Core.Wpf.Views;
using Fluent;
using Microsoft.Practices.Unity;
using Prism.Regions;

namespace Shell
{
    /// <summary>
    /// Interaction logic for ShellWindow.xaml
    /// </summary>
    public partial class ShellWindow
    {
        private readonly Dictionary<string, bool> ribbonTabVisibility = new Dictionary<string, bool>(); 
        
        public ShellWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            ribbon.Tabs.CollectionChanged += RibbonTabsOnCollectionChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
            try
            {
                var isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
                using (var reader = new StreamReader(new IsolatedStorageFileStream(typeof(ShellWindow).FullName, FileMode.OpenOrCreate, isolatedStorage)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }
                        var words = line.Split(' ');
                        ribbonTabVisibility.Add(words[0], bool.Parse(words[1]));
                    }
                }
            }
            catch
            {
                //Do nothing as this is an optional step
            }
        }

        private void RibbonTabsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
            {
                return;
            }
            foreach (var newTab in e.NewItems.Cast<RibbonTabItem>())
            {
                bool value;
                var isVisible = !ribbonTabVisibility.TryGetValue(GetSettingName(newTab), out value) || value;
                newTab.Visibility = isVisible == false ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        [Dependency]
        public ShellWindowViewModel ShellWindowViewModel
        {
            get { return DataContext as ShellWindowViewModel; }
            set { DataContext = value; }
        }

        protected override void OnClosed(EventArgs e)
        {
            ribbon.Tabs.CollectionChanged -= RibbonTabsOnCollectionChanged;
            foreach (var ribbonTab in ribbon.Tabs)
            {
                ribbonTabVisibility[GetSettingName(ribbonTab)] = ribbonTab.Visibility == Visibility.Visible;
            }
            try
            {
                var isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
                using (var writer = new StreamWriter(new IsolatedStorageFileStream(typeof (ShellWindow).FullName, FileMode.Create, isolatedStorage)))
                {
                    foreach (var item in ribbonTabVisibility)
                    {
                        writer.WriteLine("{0} {1}", item.Key, item.Value);
                    }
                }
            }
            catch
            {
                //Do nothing as this is an optional step
            }
            base.OnClosed(e);
        }

        private string GetSettingName(RibbonTabItem ribbonTab)
        {
            return string.IsNullOrEmpty(ribbonTab.Name) ? ribbonTab.GetType().FullName : ribbonTab.Name;
        }
    }
}
