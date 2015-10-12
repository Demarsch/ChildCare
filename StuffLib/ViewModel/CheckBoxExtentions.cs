using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StuffLib
{
    public class CheckBoxExtentions : CheckBox
    {
        private static Dictionary<CheckBox, string> ElementToGroupNames = new Dictionary<CheckBox, string>();

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.RegisterAttached("GroupName", typeof(string), typeof(CheckBoxExtentions), new PropertyMetadata(string.Empty, OnGroupNameChanged));

        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var checkBox = d as CheckBox;
            if (checkBox != null)
            {
                string newGroupName = e.NewValue.ToString();
                string oldGroupName = e.OldValue.ToString();
                if (string.IsNullOrEmpty(newGroupName))
                {
                    ElementToGroupNames.Remove(checkBox);
                }
                else
                {
                    if (newGroupName != oldGroupName)
                    {
                        if (!string.IsNullOrEmpty(oldGroupName))
                        {
                            ElementToGroupNames.Remove(checkBox);
                        }
                        ElementToGroupNames.Add(checkBox, newGroupName);
                        checkBox.Checked += toggleButton_Checked;
                    }
                }
            }
        }

        static void toggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = e.OriginalSource as CheckBox;
            foreach (var item in ElementToGroupNames)
            {
                if (item.Key != checkBox && item.Value == ElementToGroupNames[checkBox])
                    item.Key.IsChecked = false;
            }
        }
    }
}
