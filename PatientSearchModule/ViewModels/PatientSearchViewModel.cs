using Prism.Mvvm;

namespace PatientSearchModule.ViewModels
{
    public class PatientSearchViewModel : BindableBase
    {
        public PatientSearchViewModel()
        {
            Header = "Поиск Пациента";
        }

        private string searchText;

        public string SearchText
        {
            get { return searchText; }
            set { SetProperty(ref searchText, value); }
        }

        private object header;

        public object Header
        {
            get { return header; }
            set { SetProperty(ref header, value); }
        }
    }
}
