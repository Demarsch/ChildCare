using GalaSoft.MvvmLight;

namespace Core
{
    public class CheckedListItem : ObservableObject
    {
        public int Id { get; set; }

        public string Name { get; set; }

        private bool isChecked;

        public bool IsChecked
        {
            get { return isChecked; }
            set { Set("IsChecked", ref isChecked, value); }
        }
    }
}
