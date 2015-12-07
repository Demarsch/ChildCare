using Prism.Mvvm;

namespace Shared.Patient.ViewModels
{
    public class MediaInformation : BindableBase
    {
        private string displayName;

        public string DisplayName
        {
            get { return displayName; }
            set { SetProperty(ref displayName, value); }
        }

        private string usbId;

        public string UsbId
        {
            get { return usbId; }
            set { SetProperty(ref usbId, value); }
        }
    }
}
