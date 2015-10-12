using GalaSoft.MvvmLight;

namespace Core
{
    [PropertyChanged.ImplementPropertyChanged]
    public class PersonRelativeDTO : ObservableObject
    {
        public int PersonId { get; set; }

        public int RelativePersonId { get; set; }

        public string ShortName { get; set; }

        public int RelativeRelationId { get; set; }

        public bool IsRepresentative { get; set; }

        public string PhotoUri { get; set; }
    }
}
