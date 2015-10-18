using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interactivity;

namespace Core.Wpf.Behaviors
{
    public class TextHighlightBehavior : Behavior<TextBlock>
    {
        public static readonly DependencyProperty HighlightStyleProperty = DependencyProperty.Register("HighlightStyle", typeof(Style), typeof(TextHighlightBehavior), new PropertyMetadata(HighlightStyleChanged));

        public Style HighlightStyle
        {
            get { return (Style)GetValue(HighlightStyleProperty); }
            set { SetValue(HighlightStyleProperty, value); }
        }

        public static readonly DependencyProperty WordsToHighlightProperty = DependencyProperty.Register("WordsToHighlight", typeof(IEnumerable<string>), typeof(TextHighlightBehavior), new PropertyMetadata(WordsToHighlightChanged));

        public IEnumerable<string> WordsToHighlight
        {
            get { return (IEnumerable<string>)GetValue(WordsToHighlightProperty); }
            set { SetValue(WordsToHighlightProperty, value); }
        }

        protected override void OnAttached()
        {
            originalText = AssociatedObject.Text;
            DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock)).AddValueChanged(AssociatedObject, TextChanged);
            ParseAndHighlight(this);
            base.OnAttached();
        }

        private string originalText;

        private void TextChanged(object sender, EventArgs eventArgs)
        {
            ParseAndHighlight(this);
        }

        private static void HighlightStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ParseAndHighlight(d as TextHighlightBehavior);
        }

        private static void WordsToHighlightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ParseAndHighlight(d as TextHighlightBehavior);
        }

        private bool textChangeComesFromHighlighting;

        private static void ParseAndHighlight(TextHighlightBehavior behavior)
        {
            if (behavior.AssociatedObject == null || behavior.textChangeComesFromHighlighting)
            {
                return;
            }
            string[] words;
            string text;
            Style hightlightStyle;
            if ((hightlightStyle = behavior.HighlightStyle) == null
                || behavior.WordsToHighlight == null
                || (words = behavior.WordsToHighlight.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray()).Length == 0
                || string.IsNullOrWhiteSpace(text = behavior.AssociatedObject.Text)
                || words.Length == 0)
            {
                behavior.AssociatedObject.Text = behavior.originalText;
                behavior.textChangeComesFromHighlighting = false;
                return;
            }
            //First we fill dictionary where key is a start index of the word and value is the length of this word
            var wordStart = new SortedDictionary<int, int>();
            foreach (var word in words)
            {
                var index = -1;
                while (true)
                {
                    index = text.IndexOf(word, index == -1 ? 0 : index + 1, StringComparison.CurrentCultureIgnoreCase);
                    if (index != -1)
                    {
                        wordStart[index] = wordStart.ContainsKey(index) ? Math.Max(wordStart[index], word.Length) : word.Length;
                        continue;
                    }
                    break;
                }
            }
            //If we didn't find any word to highlight we return original text
            if (wordStart.Count == 0)
            {
                behavior.AssociatedObject.Text = behavior.originalText;
                return;
            }
            behavior.textChangeComesFromHighlighting = true;
            behavior.originalText = text;
            behavior.AssociatedObject.Inlines.Clear();
            var previousHighlightAreEnd = 0;
            foreach (var start in wordStart)
            {
                //This word start in already highlighted area thus we ignore it
                if (start.Key < previousHighlightAreEnd)
                {
                    continue;
                }
                //This word starts at the end of previous highlighted area. We should highlight current area
                if (start.Key == previousHighlightAreEnd)
                {
                    behavior.AssociatedObject.Inlines.Add(new Run(text.Substring(start.Key, start.Value)) { Style = hightlightStyle });
                }
                //This word starts after the end of previous hightlighted area. We should first add normal area between the highlighted ones
                else
                {
                    behavior.AssociatedObject.Inlines.Add(text.Substring(previousHighlightAreEnd, start.Key - previousHighlightAreEnd));
                    behavior.AssociatedObject.Inlines.Add(new Run(text.Substring(start.Key, start.Value)) { Style = hightlightStyle });
                }
                previousHighlightAreEnd = start.Key + start.Value;
            }
            //If we have trailing normal area we should add it
            if (previousHighlightAreEnd < text.Length)
            {
                behavior.AssociatedObject.Inlines.Add(text.Substring(previousHighlightAreEnd));
            }
            behavior.textChangeComesFromHighlighting = false;
        }
    }
}
