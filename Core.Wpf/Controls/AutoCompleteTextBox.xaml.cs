using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using Core.Misc;
using Core.Wpf.Misc;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace Core.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for AutoCompleteTextBox.xaml
    /// </summary>
    public partial class AutoCompleteTextBox
    {
        public AutoCompleteTextBox()
        {
            InitializeComponent();
            popup.MaxHeight = Screen.PrimaryScreen.WorkingArea.Height / 5.0;
        }

        #region SelectedItem

        public static readonly DependencyProperty SelectedItemProperty = Selector.SelectedItemProperty.AddOwner(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        private bool doNotUpdateText;

        private string GetDisplayMember(object item)
        {
            if (item == null)
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(DisplayMemberPath))
            {
                return item.ToString();
            }
            try
            {
                var dispayMember = item.GetType().GetProperty(DisplayMemberPath).GetValue(item);
                return (dispayMember ?? string.Empty).ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool doNotUpdateSelectedItem;

        private void ListBoxItemOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                SelectedItem = ((ListBoxItem)sender).DataContext;
                popup.IsOpen = false;
                if (textBox.IsKeyboardFocused)
                {
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }

        private void ListBoxItemOnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectedItem = ((ListBoxItem)sender).DataContext;
                popup.IsOpen = false;
                if (textBox.IsKeyboardFocused)
                {
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var autoComplete = (AutoCompleteTextBox)d;
            autoComplete.hasSelectedItemSign.Visibility = e.NewValue == null ? Visibility.Collapsed : Visibility.Visible;
            if (autoComplete.doNotUpdateText)
            {
                return;
            }
            autoComplete.doNotUpdateSelectedItem = true;
            autoComplete.Text = autoComplete.GetDisplayMember(e.NewValue);
            autoComplete.doNotUpdateSelectedItem = false;
            autoComplete.popup.IsOpen = false;
        }


        #endregion

        #region Text

        public static readonly DependencyProperty SearchDelayProperty = DependencyProperty.Register("SearchDelay", typeof (TimeSpan), typeof (AutoCompleteTextBox), new PropertyMetadata(AppConfiguration.UserInputDelay));

        public TimeSpan SearchDelay
        {
            get { return (TimeSpan)GetValue(SearchDelayProperty); }
            set { SetValue(SearchDelayProperty, value); }
        }

        public static readonly DependencyProperty DisplayMemberPathProperty = ItemsControl.DisplayMemberPathProperty.AddOwner(typeof(AutoCompleteTextBox), new PropertyMetadata(OnDisplayMemberPathChanged));

        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }
        private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var autoComplete = (AutoCompleteTextBox)d;
            if (autoComplete.SelectedItem != null)
            {
                autoComplete.Text = autoComplete.GetDisplayMember(autoComplete.SelectedItem);
            }
        }

        public static readonly DependencyProperty MinTextLengthToSearchProperty = DependencyProperty.Register("MinTextLengthToSearch", typeof (int), typeof (AutoCompleteTextBox), new PropertyMetadata(AppConfiguration.UserInputSearchThreshold));

        public int MinTextLengthToSearch
        {
            get { return (int)GetValue(MinTextLengthToSearchProperty); }
            set { SetValue(MinTextLengthToSearchProperty, value); }
        }

        public static readonly DependencyProperty MaxSuggestionCountProperty = DependencyProperty.Register("MaxSuggestionCount", typeof (int), typeof (AutoCompleteTextBox), new PropertyMetadata(AppConfiguration.SearchResultTakeTopCount));

        public int MaxSuggestionCount
        {
            get { return (int)GetValue(MaxSuggestionCountProperty); }
            set { SetValue(MaxSuggestionCountProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = TextBox.TextProperty.AddOwner(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private readonly object searchLocker = new object();

        private CancellationTokenSource tokenSource;

        private CancellationToken GetNewToken()
        {
            lock (searchLocker)
            {
                if (tokenSource != null)
                {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                }
                tokenSource = new CancellationTokenSource();
                return tokenSource.Token;
            }
        }

        private void CancelSearchAndClearItems()
        {
            lock (searchLocker)
            {
                if (tokenSource != null)
                {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                    tokenSource = null;
                }
            }
            listBox.ItemsSource = null;
        }

        private bool startNewSearchAfterTextChanged;

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var autoComplete = (AutoCompleteTextBox)d;
            autoComplete.startNewSearchAfterTextChanged = true;
            autoComplete.textBox.Text = (string)e.NewValue;
            autoComplete.startNewSearchAfterTextChanged = false;
        }

        private void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            Text = textBox.Text;
            //Anyway selected item is set to null
            doNotUpdateText = true;
            if (!doNotUpdateSelectedItem)
            {
                SelectedItem = null;
                listBox.ItemsSource = null;
            }
            doNotUpdateText = false;
            if (startNewSearchAfterTextChanged)
            {
                return;
            }
            //When user enters text manually
            popup.IsOpen = SearchIsAvaialable();
            noItemsFoundTextBlock.Visibility = Visibility.Collapsed;
            if (SearchIsAvaialable())
            {
                popup.IsOpen = true;
                busyContentControl.Visibility = Visibility.Visible;
                WaitAndRunSearchAsync();
            }
            else
            {
                popup.IsOpen = false;
                busyContentControl.Visibility = Visibility.Collapsed;
                CancelSearchAndClearItems();
            }
        }

        private bool SearchIsAvaialable()
        {
            return SuggestionProvider != null && !string.IsNullOrWhiteSpace(Text) && (MinTextLengthToSearch == 0 || Text.Trim().Length >= MinTextLengthToSearch);
        }

        private async void WaitAndRunSearchAsync()
        {
            var token = GetNewToken();
            try
            {
                listBox.ItemsSource = null;
                await Task.Delay(SearchDelay, token);
                var text = Text.Trim();
                var suggestions = await SearchAsync(text, token);
                busyContentControl.Visibility = Visibility.Collapsed;
                listBox.ItemsSource = suggestions;
                if (listBox.ItemsSource == null || !listBox.ItemsSource.Cast<object>().Any())
                {
                    noItemsFoundTextBlock.Visibility = Visibility.Visible;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
                //TODO: log or show exception
            }
        }

        private async Task<IEnumerable> SearchAsync(string filter, CancellationToken token)
        {
            var suggestionProvider = SuggestionProvider;
            var maxSuggestionCount = MaxSuggestionCount;
            var result = await Task.Factory.StartNew(() => suggestionProvider.GetSuggestions(filter).Cast<object>().Take(maxSuggestionCount), token);
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
            return result;
        }

        #endregion

        #region Popup

        private void PopupOnClosed(object sender, EventArgs eventArgs)
        {
            Mouse.RemovePreviewMouseUpOutsideCapturedElementHandler(this, OnMouseUpOutsideUserControl);
            ReleaseMouseCapture();
        }

        private void PopupOnOpened(object sender, EventArgs eventArgs)
        {
            Mouse.Capture(this, CaptureMode.SubTree);
            Mouse.AddPreviewMouseUpOutsideCapturedElementHandler(this, OnMouseUpOutsideUserControl);
        }

        private void OnMouseUpOutsideUserControl(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == e.OriginalSource && ReferenceEquals(e.Source, this))
            {
                popup.IsOpen = false;
            }
        }

        #endregion

        public static readonly DependencyProperty SuggestionProviderProperty = DependencyProperty.Register("SuggestionProvider", typeof(ISuggestionProvider), typeof(AutoCompleteTextBox), new PropertyMetadata(default(ISuggestionProvider)));

        public ISuggestionProvider SuggestionProvider
        {
            get { return (ISuggestionProvider)GetValue(SuggestionProviderProperty); }
            set { SetValue(SuggestionProviderProperty, value); }
        }

        public static readonly DependencyProperty LoadContentProperty = DependencyProperty.Register("LoadContent", typeof(object), typeof(AutoCompleteTextBox), new PropertyMetadata(null));

        public object LoadContent
        {
            get { return GetValue(LoadContentProperty); }
            set { SetValue(LoadContentProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty = ItemsControl.ItemTemplateProperty.AddOwner(typeof (AutoCompleteTextBox));
        
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(string), typeof(AutoCompleteTextBox), new PropertyMetadata(default(string)));

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        private void TextBoxOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenPopupIfHasSuggestions();
        }

        private bool OpenPopupIfHasSuggestions()
        {
            if (listBox.ItemsSource != null && listBox.ItemsSource.Cast<object>().Any() && !popup.IsOpen)
            {
                popup.IsOpen = true;
                return true;
            }
            return false;
        }

        private void TextBoxOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                if (OpenPopupIfHasSuggestions())
                {
                    e.Handled = true;
                }
            }
        }
    }
}
