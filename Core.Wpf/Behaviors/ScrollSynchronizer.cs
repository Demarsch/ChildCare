using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Core.Wpf.Behaviors
{
    public class ScrollSynchronizer : DependencyObject
    {
        public static readonly DependencyProperty HorizontalScrollGroupProperty = DependencyProperty.RegisterAttached("HorizontalScrollGroup", typeof(string), typeof(ScrollSynchronizer),
                                                                                                                      new PropertyMetadata(OnHorizontalScrollGroupChanged));

        public static void SetHorizontalScrollGroup(DependencyObject obj, string scrollGroup)
        {
            obj.SetValue(HorizontalScrollGroupProperty, scrollGroup);
        }

        public static string GetHorizontalScrollGroup(DependencyObject obj)
        {
            return (string)obj.GetValue(HorizontalScrollGroupProperty);
        }

        public static readonly DependencyProperty VerticalScrollGroupProperty = DependencyProperty.RegisterAttached("VerticalScrollGroup", typeof(string), typeof(ScrollSynchronizer),
                                                                                                                    new PropertyMetadata(OnVerticalScrollGroupChanged));

        public static void SetVerticalScrollGroup(DependencyObject element, string value)
        {
            element.SetValue(VerticalScrollGroupProperty, value);
        }

        public static string GetVerticalScrollGroup(DependencyObject element)
        {
            return (string)element.GetValue(VerticalScrollGroupProperty);
        }

        private static readonly Dictionary<ScrollViewer, string> HorizontalScrollViewers = new Dictionary<ScrollViewer, string>();

        private static readonly Dictionary<ScrollViewer, string> VerticalScrollViewers = new Dictionary<ScrollViewer, string>();

        private static readonly Dictionary<string, double> HorizontalScrollOffsets = new Dictionary<string, double>();

        private static readonly Dictionary<string, double> VerticalScrollOffsets = new Dictionary<string, double>();

        private static void OnVerticalScrollGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer;
            if (scrollViewer == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty((string)e.OldValue) && VerticalScrollViewers.ContainsKey(scrollViewer))
            {
                scrollViewer.ScrollChanged -= VerticalScrollViewer_ScrollChanged;
                VerticalScrollViewers.Remove(scrollViewer);
            }
            if (string.IsNullOrEmpty((string)e.NewValue))
            {
                return;
            }
            // If group already exists, set scrollposition of new scrollviewer to the scrollposition of the group
            if (VerticalScrollOffsets.Keys.Contains((string)e.NewValue))
            {
                scrollViewer.ScrollToVerticalOffset(VerticalScrollOffsets[(string)e.NewValue]);
            }
            else
            {
                VerticalScrollOffsets.Add((string)e.NewValue, scrollViewer.VerticalOffset);
            }
            // Add scrollviewer
            VerticalScrollViewers.Add(scrollViewer, (string)e.NewValue);
            scrollViewer.ScrollChanged += VerticalScrollViewer_ScrollChanged;
        }

        private static void OnHorizontalScrollGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer;
            if (scrollViewer == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty((string)e.OldValue) && HorizontalScrollViewers.ContainsKey(scrollViewer))
            {
                scrollViewer.ScrollChanged -= HorizontalScrollViewer_ScrollChanged;
                HorizontalScrollViewers.Remove(scrollViewer);
            }
            if (string.IsNullOrEmpty((string)e.NewValue))
            {
                return;
            }
            // If group already exists, set scrollposition of new scrollviewer to the scrollposition of the group
            if (HorizontalScrollOffsets.Keys.Contains((string)e.NewValue))
            {
                scrollViewer.ScrollToHorizontalOffset(HorizontalScrollOffsets[(string)e.NewValue]);
            }
            else
            {
                HorizontalScrollOffsets.Add((string)e.NewValue, scrollViewer.HorizontalOffset);
            }
            // Add scrollviewer
            HorizontalScrollViewers.Add(scrollViewer, (string)e.NewValue);
            scrollViewer.ScrollChanged += HorizontalScrollViewer_ScrollChanged;
        }

        private static void HorizontalScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.HorizontalChange != 0)
            {
                var changedScrollViewer = sender as ScrollViewer;
                Scroll(changedScrollViewer, true);
            }
        }

        private static void VerticalScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                var changedScrollViewer = sender as ScrollViewer;
                Scroll(changedScrollViewer, false);
            }
        }

        private static void Scroll(ScrollViewer changedScrollViewer, bool isHorizontal)
        {
            var group = isHorizontal ? HorizontalScrollViewers[changedScrollViewer] : VerticalScrollViewers[changedScrollViewer];
            if (isHorizontal)
            {
                HorizontalScrollOffsets[group] = changedScrollViewer.HorizontalOffset;
            }
            else
            {
                VerticalScrollOffsets[group] = changedScrollViewer.VerticalOffset;
            }
            if (isHorizontal)
            {
                foreach (var scrollViewer in HorizontalScrollViewers.Where(s => s.Value == @group && s.Key != changedScrollViewer)
                                                                    .Where(scrollViewer => scrollViewer.Key.HorizontalOffset != changedScrollViewer.HorizontalOffset))
                {
                    scrollViewer.Key.ScrollToHorizontalOffset(changedScrollViewer.HorizontalOffset);
                }
            }
            else
            {
                foreach (var scrollViewer in VerticalScrollViewers.Where(s => s.Value == @group && s.Key != changedScrollViewer)
                                                                  .Where(scrollViewer => scrollViewer.Key.VerticalOffset != changedScrollViewer.VerticalOffset))
                {
                    scrollViewer.Key.ScrollToVerticalOffset(changedScrollViewer.VerticalOffset);
                }
            }
        }
    }
}