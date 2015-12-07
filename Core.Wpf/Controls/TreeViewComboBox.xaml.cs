using System;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Core.Misc;
using Core.Wpf.Converters;
using Core.Wpf.Extensions;
using Fluent;
using Xceed.Wpf.Toolkit.Core.Utilities;
using ComboBox = System.Windows.Controls.ComboBox;

namespace Core.Wpf.Controls
{
    /// <summary>
    ///     Interaction logic for TreeViewComboBox.xaml
    /// </summary>
    public partial class TreeViewComboBox
    {
        public static readonly Predicate<object> SelectAnyPredicate = x => true;

        public static readonly Predicate<object> SelectBottomLevelOnlyPredicate = x => { throw new NotImplementedException(); };

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof (IEnumerable), typeof (TreeViewComboBox), new PropertyMetadata(default(IEnumerable)));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof (object), typeof (TreeViewComboBox), new FrameworkPropertyMetadata(null, 
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnSelectedItemChanged));

        public static readonly DependencyProperty SelectionPredicateProperty = DependencyProperty.Register("SelectionPredicate",
            typeof (Predicate<object>),
            typeof (TreeViewComboBox),
            new PropertyMetadata(SelectBottomLevelOnlyPredicate));

        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPathPath", typeof (string), typeof (TreeViewComboBox), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof (DataTemplate), typeof (TreeViewComboBox), new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register("MaxDropDownHeight", typeof (double), typeof (TreeViewComboBox), new PropertyMetadata(SystemParameters.PrimaryScreenHeight / 3));

        private Stack hierarchyStack;

        private bool ignoreSelectionChanged;

        private TreeViewItem itemToBeSelected;

        public TreeViewComboBox()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var isInRibbon = this.FindAncestor<Ribbon>() != null;
            var comboBox = isInRibbon ? new Fluent.ComboBox { IsEditable = false } : new ComboBox();
            comboBox.MaxDropDownHeight = 0.0;
            var binding = new Binding("DisplayMemberPath") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) { AncestorType = typeof(UserControl) } };
            comboBox.SetBinding(ItemsControl.DisplayMemberPathProperty, binding);
            binding = new Binding("IsOpen") { Source = popup, Converter = ReversedBoolConverter.Instance };
            comboBox.SetBinding(IsHitTestVisibleProperty, binding);
            comboBox.DropDownOpened += ComboBoxOnDropDownOpened;
            comboBoxPlaceholder.Content = comboBox;
            popup.PlacementTarget = comboBox;
        }

        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public Predicate<object> SelectionPredicate
        {
            get { return (Predicate<object>)GetValue(SelectionPredicateProperty); }
            set { SetValue(SelectionPredicateProperty, value); }
        }

        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        private bool SelectBottomLevelOnly(object item)
        {
            if (itemToBeSelected == null)
            {
                return false;
            }
            return !itemToBeSelected.HasItems;
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TreeViewComboBox)d;
            //TODO: hierarchical validation that new item exists in the tree
            control.SelecteItemInCombobox(e.NewValue);
            if (control.popup.IsOpen)
            {
                control.SelectItemInTreeView(e.NewValue);
            }
            else
            {
                control.popup.Opened += control.PopupOnOpened;
            }
        }

        private void PopupOnOpened(object sender, EventArgs eventArgs)
        {
            popup.Opened -= PopupOnOpened;
            SelectItemInTreeView(SelectedItem);
        }

        private void SelectItemInTreeView(object newSelectedItem)
        {
            if (newSelectedItem == null)
            {
                ((ComboBox)comboBoxPlaceholder.Content).ItemsSource = null;
            }
            var currentItem = newSelectedItem as IHierarchyItem;
            hierarchyStack = new Stack();
            while (currentItem != null)
            {
                hierarchyStack.Push(currentItem);
                currentItem = currentItem.Parent;
            }
            WaitForItemGenerationAndSelectItem(treeView.ItemContainerGenerator);
        }

        private void WaitForItemGenerationAndSelectItem(ItemContainerGenerator itemContainerGenerator)
        {
            TreeViewItem treeViewItem = null;
            while (hierarchyStack.Count > 0)
            {
                var item = hierarchyStack.Peek();
                treeViewItem = (TreeViewItem)itemContainerGenerator.ContainerFromItem(item);
                if (treeViewItem == null)
                {
                    itemContainerGenerator.StatusChanged += ItemContainerGeneratorOnStatusChanged;
                    return;
                }
                hierarchyStack.Pop();
                if (hierarchyStack.Count > 0)
                {
                    treeViewItem.IsExpanded = true;
                }
                itemContainerGenerator = treeViewItem.ItemContainerGenerator;
            }
            if (treeViewItem == null)
            {
                return;
            }
            treeViewItem.BringIntoView();
            ignoreSelectionChanged = true;
            treeViewItem.IsSelected = true;
            ignoreSelectionChanged = false;
        }

        private void ItemContainerGeneratorOnStatusChanged(object sender, EventArgs eventArgs)
        {
            var itemContainerGenerator = (ItemContainerGenerator)sender;
            if (itemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                return;
            }
            itemContainerGenerator.StatusChanged -= ItemContainerGeneratorOnStatusChanged;
            WaitForItemGenerationAndSelectItem(itemContainerGenerator);
        }

        private void TreeViewOnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ignoreSelectionChanged)
            {
                return;
            }
            var selectionPredicate = SelectionPredicate == SelectBottomLevelOnlyPredicate ? SelectBottomLevelOnly : SelectionPredicate;
            if (selectionPredicate(e.NewValue))
            {
                SelecteItemInCombobox(e.NewValue);
                popup.IsOpen = false;
            }
        }

        private void SelecteItemInCombobox(object newValue)
        {
            var comboBox = (ComboBox)comboBoxPlaceholder.Content;
            comboBox.ItemsSource = new[] { newValue };
            comboBox.SelectedIndex = 0;
            SelectedItem = newValue;
        }

        private void ComboBoxOnDropDownOpened(object sender, EventArgs eventArgs)
        {
            var comboBox = (ComboBox)sender;
            comboBox.IsDropDownOpen = false;
            new DispatcherTimer(TimeSpan.FromSeconds(0.1), DispatcherPriority.Normal, OpenPopupCallback, Dispatcher).Start();
        }

        private void OpenPopupCallback(object sender, EventArgs eventArgs)
        {
            ((DispatcherTimer)sender).Stop();
            popup.IsOpen = true;
            treeView.Focus();
            Mouse.Capture(treeView, CaptureMode.SubTree);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(treeView, TreeViewOnMouseDown);
        }

        private void TreeViewOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(treeView, TreeViewOnMouseDown);
            treeView.ReleaseMouseCapture();
            popup.IsOpen = false;
            mouseButtonEventArgs.Handled = true;
        }

        private void TreeView_OnSelected(object sender, RoutedEventArgs e)
        {
            itemToBeSelected = e.OriginalSource as TreeViewItem;
        }
    }
}