using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using StuffLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MainLib
{
    public class EditPersonCommonDataViewModel : ObservableObject, IDataErrorInfo
    {
        #region Fields

        private readonly IPersonService service;

        private readonly IDialogService dialogService;

        private readonly IDocumentService documentService;

        private PersonName personName;

        #endregion

        #region Constructors

        public EditPersonCommonDataViewModel(Person person, IPersonService service, IDialogService dialogService, IDocumentService documentService)
        {
            if (service == null)
                throw new ArgumentNullException("service");
            if (dialogService == null)
                throw new ArgumentNullException("dialogService");
            if (dialogService == null)
                throw new ArgumentNullException("documentService");

            this.documentService = documentService;
            this.dialogService = dialogService;
            this.service = service;
            this.person = person;

            ChangeNameReasons = new ObservableCollection<ChangeNameReason>(service.GetActualChangeNameReasons());
            Genders = new ObservableCollection<Gender>(service.GetGenders());
            TakePhotoCommand = new RelayCommand(TakePhoto);
        }

        #endregion

        #region Properties

        private Person person;
        public Person Person
        {
            get { return person; }
            set
            {
                Set(() => Person, ref person, value);
                if (!IsEmpty)
                {
                    FillPersonData();
                }
            }
        }

        public bool IsEmpty
        {
            get { return person == null; }
        }

        private int photoId = 0;
        public int PhotoId
        {
            get { return photoId; }
            set { Set(() => PhotoId, ref photoId, value); }
        }

        private ImageSource photoURI;
        public ImageSource PhotoURI
        {
            get
            {
                return photoURI;
            }
            set
            {
                Uri imageUri;
                BitmapImage bitmapImage = new BitmapImage();
                if (value == null)
                {

                    //ToDo: make this property connecteted with actual value
                    switch (GenderId)
                    {
                        case 1:
                            imageUri = new Uri("pack://application:,,,/Resources;component/Images/Man48x48.png");
                            break;
                        case 2:
                            imageUri = new Uri("pack://application:,,,/Resources;component/Images/Woman48x48.png");
                            break;
                        default:
                            imageUri = new Uri("pack://application:,,,/Resources;component/Images/Question48x48.png");
                            break;
                    }
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = imageUri;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    Set(() => PhotoURI, ref photoURI, bitmapImage);
                }
                else
                    Set(() => PhotoURI, ref photoURI, value);

            }
        }

        private string lastName = string.Empty;
        public string LastName
        {
            get
            {
                return lastName;
            }
            set
            {
                Set(() => LastName, ref lastName, value);
                RaisePropertyChanged(() => IsFIOChanged);
                RaisePropertyChanged(() => IsSelectedChangeNameReasonWithCreateNewPersonNames);
            }
        }

        private string firstName = string.Empty;
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                Set(() => FirstName, ref firstName, value);
                RaisePropertyChanged(() => IsFIOChanged);
                RaisePropertyChanged(() => IsSelectedChangeNameReasonWithCreateNewPersonNames);
            }
        }

        private string middleName = string.Empty;
        public string MiddleName
        {
            get
            {
                return middleName;
            }
            set
            {
                Set(() => MiddleName, ref middleName, value);
                RaisePropertyChanged(() => IsFIOChanged);
                RaisePropertyChanged(() => IsSelectedChangeNameReasonWithCreateNewPersonNames);
            }
        }

        private DateTime birthDate = DateTime.Now;
        public DateTime BirthDate
        {
            get { return birthDate; }
            set { Set(() => BirthDate, ref birthDate, value); }
        }

        private string phones = string.Empty;
        public string Phones
        {
            get { return phones; }
            set { Set(() => Phones, ref phones, value); }
        }

        private string email = string.Empty;
        public string Email
        {
            get { return email; }
            set { Set(() => Email, ref email, value); }
        }

        private string snils = string.Empty;
        public string SNILS
        {
            get { return snils; }
            set { Set(() => SNILS, ref snils, value); }
        }

        private string medNumber = string.Empty;
        public string MedNumber
        {
            get { return medNumber; }
            set { Set(() => MedNumber, ref medNumber, value); }
        }

        private int? genderId = 0;
        public int? GenderId
        {
            get { return genderId; }
            set { Set(() => GenderId, ref genderId, value); }
        }

        public bool IsFIOChanged
        {
            get { return !IsEmpty && personName != null && (personName.LastName != LastName || personName.FirstName != FirstName || personName.MiddleName != MiddleName); }
        }

        public bool IsSelectedChangeNameReasonWithCreateNewPersonNames
        {
            get { return IsFIOChanged && SelectedChangeNameReason != null && SelectedChangeNameReason.NeedCreateNewPersonName; }
        }

        public DateTime changeNameDate;
        public DateTime ChangeNameDate
        {
            get { return changeNameDate; }
            set { Set(() => ChangeNameDate, ref changeNameDate, value); }
        }

        private ObservableCollection<Gender> genders = new ObservableCollection<Gender>();
        public ObservableCollection<Gender> Genders
        {
            get { return genders; }
            set { Set(() => Genders, ref genders, value); }
        }

        private ObservableCollection<ChangeNameReason> сhangeNameReasons = new ObservableCollection<ChangeNameReason>();
        public ObservableCollection<ChangeNameReason> ChangeNameReasons
        {
            get { return сhangeNameReasons; }
            set { Set(() => ChangeNameReasons, ref сhangeNameReasons, value); }
        }

        private ChangeNameReason selectedChangeNameReason;
        public ChangeNameReason SelectedChangeNameReason
        {
            get { return selectedChangeNameReason; }
            set
            {
                Set(() => SelectedChangeNameReason, ref selectedChangeNameReason, value);
                RaisePropertyChanged(() => IsSelectedChangeNameReasonWithCreateNewPersonNames);
            }
        }

        public string FullName
        {
            //ToDo: Maybe get current values of LastName, FirstName, MiddleName
            get { return !IsEmpty ? person.FullName : string.Empty; }
        }

        #endregion

        #region Commands

        public ICommand TakePhotoCommand { get; set; }
        private PhotoViewModel photoViewModel;
        private void TakePhoto()
        {
            if (photoViewModel == null)
                photoViewModel = new PhotoViewModel();
            var dialogResult = dialogService.ShowDialog(photoViewModel);
            if (dialogResult == true)
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                var fileData = documentService.GetBinaryDataFromImage(encoder, photoViewModel.SnapshotTaken);
                var str = string.Empty;
                if (fileData != null && fileData.Length > 0)
                    PhotoId = documentService.UploadDocument(new Document()
                        {
                            FileData = fileData,
                            Description = "фото",
                            DisplayName = "фото",
                            Extension = "jpg",
                            FileName = "фото",
                            FileSize = fileData.Length,
                            UploadDate = DateTime.Now
                        }, out str);
                PhotoURI = photoViewModel.SnapshotTaken;
                photoViewModel.SnapshotBitmap = null;
                photoViewModel.SnapshotTaken = null;
            }
        }

        #endregion

        #region Methods

        public Person SetPerson(out List<PersonName> personNames)
        {
            personNames = new List<PersonName>();
            PersonName newPersonName = null;
            if (IsFIOChanged)
            {
                if (!SelectedChangeNameReason.NeedCreateNewPersonName)
                {
                    personName.LastName = LastName;
                    personName.FirstName = FirstName;
                    personName.MiddleName = MiddleName;
                }
                else
                {
                    newPersonName = new PersonName()
                    {
                        LastName = LastName,
                        FirstName = FirstName,
                        MiddleName = MiddleName,
                        BeginDateTime = ChangeNameDate,
                        EndDateTime = DateTime.MaxValue
                    };
                    personNames.Add(newPersonName);

                    personName.EndDateTime = ChangeNameDate;
                    personName.ChangeNameReasonId = SelectedChangeNameReason.Id;
                }
            }
            else
            {
                if (personName == null)
                {
                    personName = new PersonName()
                    {
                        LastName = LastName,
                        FirstName = FirstName,
                        MiddleName = MiddleName,
                        BeginDateTime = new DateTime(1900, 1, 1),
                        EndDateTime = DateTime.MaxValue
                    };

                }
            }
            personNames.Add(personName);
            if (IsEmpty)
            {
                person = new Person
                {
                    AmbNumberString = string.Empty,
                    AmbNumber = 0,
                    Year = 0
                    //AmbCardFirstListHashCode = 0,
                    //PersonHospListHashCode = 0,
                    //RadiationListHashCode = 0
                };
            }
            person.BirthDate = BirthDate;
            person.Snils = SNILS.Replace("-", string.Empty).Replace(" ", string.Empty);
            person.MedNumber = MedNumber;
            person.GenderId = GenderId.Value;
            person.Phones = Phones;
            person.Email = Email;
            person.PhotoId = PhotoId > 0 ? PhotoId : (int?)null;

            person.ShortName = LastName + " " + FirstName.Substring(0, 1) + ". " + (MiddleName != string.Empty ? MiddleName.Substring(0, 1) + "." : string.Empty);
            person.FullName = LastName + " " + FirstName + " " + MiddleName;

            RaisePropertyChanged(() => IsFIOChanged);
            RaisePropertyChanged(() => IsSelectedChangeNameReasonWithCreateNewPersonNames);

            return person;
        }

        private void FillPersonData()
        {
            if (!IsEmpty)
            {
                personName = service.GetActualPersonName(person.Id);
                if (personName != null)
                {
                    LastName = personName.LastName;
                    FirstName = personName.FirstName;
                    MiddleName = personName.MiddleName;
                }
                PhotoId = person.PhotoId.ToInt();
                if (person.PhotoId.HasValue)
                {
                    var photo = documentService.GetDocumentById(person.PhotoId.Value);
                    if (photo == null)
                        PhotoURI = null;
                    else
                        PhotoURI = documentService.GetImageSourceFromBinaryData(photo.FileData);
                }
                else
                    PhotoURI = null;
                BirthDate = person.BirthDate;
                SNILS = person.Snils;
                MedNumber = person.MedNumber;
                GenderId = person.GenderId;
            }
            else
            {
                LastName = string.Empty;
                FirstName = string.Empty;
                MiddleName = string.Empty;
                BirthDate = DateTime.Now;
                SNILS = string.Empty;
                MedNumber = string.Empty;
                GenderId = 0;
                PhotoURI = null;
            }
        }

        #endregion

        #region Implementation IDataErrorInfo

        private bool saveWasRequested { get; set; }

        public readonly HashSet<string> invalidProperties = new HashSet<string>();

        public bool Invalidate()
        {
            saveWasRequested = true;
            RaisePropertyChanged(string.Empty);
            return invalidProperties.Count < 1;
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                if (columnName == "SelectedChangeNameReason")
                {
                    result = IsFIOChanged && SelectedChangeNameReason == null ? "Укажите причину изменения ФИО" : string.Empty;
                }
                if (columnName == "ChangeNameDate")
                {
                    result = IsFIOChanged && SelectedChangeNameReason != null && SelectedChangeNameReason.NeedCreateNewPersonName && ChangeNameDate == null ? "Укажите дату, с которой изменилось ФИО" : string.Empty;
                }
                if (columnName == "GenderId")
                {
                    result = !GenderId.HasValue ? "Укажите пол" : string.Empty;
                }
                if (columnName == "SNILS")
                {
                    result = SNILS.FilterChars().Length > 0 && SNILS.FilterChars().Length != 11 ? "Поле СНИЛС должно содержать 11 цифр" : string.Empty;
                }
                if (columnName == "MedNumber")
                {

                    result = MedNumber.FilterChars().Length > 0 && MedNumber.FilterChars().Length != 16 ? "Поле СНИЛС должно содержать 16 цифр" : string.Empty;
                }
                if (string.IsNullOrEmpty(result))
                {
                    invalidProperties.Remove(columnName);
                }
                else
                {
                    invalidProperties.Add(columnName);
                }
                return result;
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
