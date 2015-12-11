using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Core.Extensions;
using Core.Misc;
using Core.Wpf.Converters;
using Core.Wpf.Extensions;
using Core.Wpf.Misc;
using Fluent;
using Xceed.Wpf.AvalonDock.Controls;
using Binding = System.Windows.Data.Binding;
using ComboBox = System.Windows.Controls.ComboBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace Core.Wpf.Controls
{
    /// <summary>
    ///     Interaction logic for TreeViewComboBox.xaml
    /// </summary>
    public partial class TreeViewComboBox
    {
        public static readonly Predicate<object> SelectBottomLevelOnlyPredicate = x => { throw new NotImplementedException(); };

        public static readonly Func<object, string, bool> FilterBasedOnDisplayMemberPathPredicate = (x, y) => { throw new NotImplementedException(); };

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(TreeViewComboBox), new PropertyMetadata(default(IEnumerable)));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(TreeViewComboBox), new FrameworkPropertyMetadata(null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnSelectedItemChanged));

        public static readonly DependencyProperty SelectionPredicateProperty = DependencyProperty.Register("SelectionPredicate",
            typeof(Predicate<object>),
            typeof(TreeViewComboBox),
            new PropertyMetadata(SelectBottomLevelOnlyPredicate));

        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(TreeViewComboBox), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(TreeViewComboBox), new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register("MaxDropDownHeight", typeof(double), typeof(TreeViewComboBox), new PropertyMetadata(SystemParameters.PrimaryScreenHeight / 3));

        public static readonly DependencyProperty FilterPredicateProperty = DependencyProperty.Register("FilterPredicate", 
            typeof(Func<object, string, bool>), 
            typeof(TreeViewComboBox), 
            new PropertyMetadata(FilterBasedOnDisplayMemberPathPredicate));

        private static object CoerceFilterValueCallback(DependencyObject dependencyObject, object baseValue)
        {
            var result = baseValue as string ?? string.Empty;
            return string.IsNullOrWhiteSpace(result) ? string.Empty : result;
        }

        public Func<object, string, bool> FilterPredicate
        {
            get { return (Func<object, string, bool>)GetValue(FilterPredicateProperty); }
            set { SetValue(FilterPredicateProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(string), typeof(TreeViewComboBox), new FrameworkPropertyMetadata(string.Empty, OnFilterChanged, CoerceFilterValueCallback));

        private bool FilterBasedOnDisplayMemberPath(object obj, string filter)
        {
            filter = filter.Trim();
            if (filter.Length < AppConfiguration.UserInputSearchThreshold)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            var objString = string.IsNullOrEmpty(DisplayMemberPath) ? obj.ToString() : obj.GetValue(DisplayMemberPath).ToString();
            return objString.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeViewComboBox = (TreeViewComboBox)d;
            var newValue = e.NewValue as string ?? string.Empty;
            newValue = newValue.Trim();
            if (newValue.Length < AppConfiguration.UserInputSearchThreshold)
            {
                treeViewComboBox.CollapseAndClearFilter(treeViewComboBox.treeView.ItemContainerGenerator);
                treeViewComboBox.NoFilteredItems = false;
            }
            else
            {
                treeViewComboBox.ExpandAndFilter(treeViewComboBox.treeView.ItemContainerGenerator);
            }
        }

        public static readonly DependencyProperty NoFilteredItemsProperty = DependencyProperty.Register(
                                                        "NoFilteredItems", typeof(bool), typeof(TreeViewComboBox), new PropertyMetadata(default(bool)));

        public bool NoFilteredItems
        {
            get { return (bool)GetValue(NoFilteredItemsProperty); }
            set { SetValue(NoFilteredItemsProperty, value); }
        }

        private void CollapseAndClearFilter(ItemContainerGenerator itemContainerGenerator)
        {
            if (itemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                return;
            }
            for (var index = 0; index < itemContainerGenerator.Items.Count; index++)
            {
                var treeViewItem = (TreeViewItem)itemContainerGenerator.ContainerFromIndex(index);
                if (treeViewItem.ItemContainerGenerator.Items.Count > 0)
                {
                    CollapseAndClearFilter(treeViewItem.ItemContainerGenerator);
                }
                treeViewItem.IsExpanded = false;
                treeViewItem.Visibility = Visibility.Visible;
            }
        }

        private void ExpandAndFilter(ItemContainerGenerator itemContainerGenerator)
        {
            if (itemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                itemContainerGenerator.StatusChanged += OnItemsGeneratedForFilter;
                return;
            }
            for (var index = 0; index < itemContainerGenerator.Items.Count; index++)
            {
                var treeViewItem = (TreeViewItem)itemContainerGenerator.ContainerFromIndex(index);
                if (treeViewItem.Items.Count == 0)
                {
                    UpdateVisibility(treeViewItem);
                }
                else
                {
                    treeViewItem.IsExpanded = true;
                    ExpandAndFilter(treeViewItem.ItemContainerGenerator);
                }
            }
        }

        private void UpdateVisibility(TreeViewItem treeViewItem)
        {
            var filterPredicate = FilterPredicate == FilterBasedOnDisplayMemberPathPredicate ? FilterBasedOnDisplayMemberPath : FilterPredicate;
            treeViewItem.Visibility = filterPredicate(treeViewItem.DataContext, Filter.Trim()) ? Visibility.Visible : Visibility.Collapsed;
            var parent = treeViewItem.FindVisualAncestor<TreeViewItem>();
            while (parent != null)
            {
                parent.Visibility = Enumerable.Range(0, parent.ItemContainerGenerator.Items.Count)
                                              .Select(x => (TreeViewItem)parent.ItemContainerGenerator.ContainerFromIndex(x))
                                              .Any(x => x.Visibility == Visibility.Visible)
                                            ? Visibility.Visible : Visibility.Collapsed;
                parent = parent.FindVisualAncestor<TreeViewItem>();
            }
            NoFilteredItems = Enumerable.Range(0, treeView.ItemContainerGenerator.Items.Count)
                                        .Select(x => treeView.ItemContainerGenerator.ContainerFromIndex(x))
                                        .Cast<TreeViewItem>()
                                        .All(x => x.Visibility == Visibility.Collapsed);

        }

        private void OnItemsGeneratedForFilter(object sender, EventArgs e)
        {
            var itemContainerGenerator = (ItemContainerGenerator)sender;
            if (itemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                return;
            }
            itemContainerGenerator.StatusChanged -= OnItemsGeneratedForFilter;
            ExpandAndFilter(itemContainerGenerator);
        }

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        private Stack hierarchyStack;

        private bool ignoreSelectionChanged;

        private TreeViewItem itemToBeSelected;

        public TreeViewComboBox()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            treeView.KeyDown += TreeViewOnPreviewKeyDown;
        }

        private void TreeViewOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                popup.IsOpen = false;
                return;
            }
            if (e.Key == Key.Back)
            {
                if (!string.IsNullOrEmpty(Filter))
                {
                    Filter = Filter.Substring(0, Filter.Length - 1);
                }
                return;
            }
            var @char = KeyToCharTranslator.GetCharFromKey(e.Key);
            if (@char != '\0')
            {
                Filter = Filter + char.ToUpper(@char);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
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
            if (isInRibbon)
            {
                var ribbonBrush = TryFindResource("ButtonHoverInnerBackgroundBrush");
                if (ribbonBrush != null)
                {
                    treeView.Resources.Add(SystemColors.HighlightBrushKey, ribbonBrush);
                }
            }
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
            ExpandAndSelectItem(treeView.ItemContainerGenerator);
        }

        private void ExpandAndSelectItem(ItemContainerGenerator itemContainerGenerator)
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
            ExpandAndSelectItem(itemContainerGenerator);
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
            Mouse.Capture(popup.Child, CaptureMode.SubTree);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(popup.Child, OnMouseDownOusidePopupContent);
        }

        private void OnMouseDownOusidePopupContent(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(popup.Child, OnMouseDownOusidePopupContent);
            popup.Child.ReleaseMouseCapture();
            popup.IsOpen = false;
            mouseButtonEventArgs.Handled = true;
        }

        private void TreeView_OnSelected(object sender, RoutedEventArgs e)
        {
            itemToBeSelected = e.OriginalSource as TreeViewItem;
        }

        private void OnClearFilterButtonClick(object sender, RoutedEventArgs e)
        {
            Filter = string.Empty;
        }

        private void PopupOnClosed(object sender, EventArgs e)
        {
            Filter = string.Empty;
        }
    }
}