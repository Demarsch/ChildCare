using Prism.Mvvm;

namespace PatientInfoModule.ViewModels
{
    public class AddressViewModel : BindableBase
    {
        private int? addressType;

        public int? AddressType
        {
            get { return addressType; }
            set { SetProperty(ref addressType, value); }
        }
    }
}
