using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using AdminModule.Model;
using AdminModule.Services;
using Core.Data;
using Core.Data.Misc;
using Core.Extensions;
using Core.Misc;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Shared.Patient.ViewModels;

namespace AdminModule.ViewModels
{
    public class UserPropertiesDialogViewModel : TrackableBindableBase, IDialogViewModel, IChangeTrackerMediator, IDisposable, IActiveDataErrorInfo
    {
        private readonly IUserAccessService userAccessService;

        private readonly IUserProvider userProvider;

        private readonly ILog log;

        private readonly IDialogServiceAsync dialogService;

        private readonly IFileService fileService;

        private readonly ICacheService cacheService;

        public UserPropertiesDialogViewModel(IUserAccessService userAccessService, 
                                             IUserProvider userProvider, 
                                             ILog log, 
                                             IDialogServiceAsync dialogService, 
                                             IFileService fileService, 
                                             ICacheService cacheService)
        {
            if (userAccessService == null)
            {
                throw new ArgumentNullException("userAccessService");
            }
            if (userProvider == null)
            {
                throw new ArgumentNullException("userProvider");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (fileService == null)
            {
                throw new ArgumentNullException("fileService");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.userAccessService = userAccessService;
            this.userProvider = userProvider;
            this.log = log;
            this.dialogService = dialogService;
            this.fileService = fileService;
            this.cacheService = cacheService;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            validator = new Validator(this);
            CloseCommand = new DelegateCommand<bool?>(Close);
            SearchAndSyncUserCommand = new DelegateCommand(SearchAndSyncUserAsync);
            SyncUserCommand = new DelegateCommand<UserInfo>(SyncUser);
            UnsyncUserCommand = new DelegateCommand(UnsyncUser);
            TakePhotoCommand = new DelegateCommand(TakePhotoAsync);
            ChangeTracker = new ChangeTrackerEx<UserPropertiesDialogViewModel>(this);
            ChangeTracker.PropertyChanged += OnChangesTracked;
            currentPersonId = SpecialValues.NonExistingId;
            loadPatientInfoCommandWrapper = new CommandWrapper
                                            {
                                                Command = new DelegateCommand(LoadPatientInfoAsync),
                                                CommandName = "Повторить"
                                            };
        }

        private void OnChangesTracked(object sender, PropertyChangedEventArgs e)
        {
            UpdateNameIsChanged();
        }

        #region Properties

        private bool isNameChanged;

        public bool IsNameChanged
        {
            get { return isNameChanged; }
            set
            {
                if (SetProperty(ref isNameChanged, value) && !value)
                {
                    IsIncorrectName = false;
                    IsNewName = false;
                    NewNameStartDate = null;
                }
            }
        }

        private void UpdateNameIsChanged()
        {
            IsNameChanged = !currentPersonId.IsNewOrNonExisting()
                            && (ChangeTracker.PropertyHasChanges(() => LastName)
                                || ChangeTracker.PropertyHasChanges(() => FirstName)
                                || ChangeTracker.PropertyHasChanges(() => MiddleName));
        }

        private bool isIncorrectName;

        public bool IsIncorrectName
        {
            get { return isIncorrectName; }
            set
            {
                if (SetProperty(ref isIncorrectName, value) && value)
                {
                    IsNewName = false;
                    NewNameStartDate = null;
                    OnPropertyChanged(() => IsNewName);
                }
            }
        }

        private bool isNewName;

        public bool IsNewName
        {
            get { return isNewName; }
            set
            {
                if (SetProperty(ref isNewName, value) && value)
                {
                    IsIncorrectName = false;
                    OnPropertyChanged(() => IsNewName);
                }
            }
        }

        private DateTime? newNameStartDate;

        public DateTime? NewNameStartDate
        {
            get { return newNameStartDate; }
            set { SetProperty(ref newNameStartDate, value); }
        }

        private string lastName;

        public string LastName
        {
            get { return lastName; }
            set { SetTrackedProperty(ref lastName, value); }
        }

        private string firstName;

        public string FirstName
        {
            get { return firstName; }
            set { SetTrackedProperty(ref firstName, value); }
        }

        private string middleName;

        public string MiddleName
        {
            get { return middleName; }
            set { SetTrackedProperty(ref middleName, value); }
        }

        private bool isMale;

        public bool IsMale
        {
            get { return isMale; }
            set { SetTrackedProperty(ref isMale, value); }
        }

        private DateTime? birthDate;

        public DateTime? BirthDate
        {
            get { return birthDate; }
            set { SetTrackedProperty(ref birthDate, value); }
        }

        #endregion

        #region Photo

        private ImageSource photoSource;

        public ImageSource PhotoSource
        {
            get { return photoSource; }
            set { SetTrackedProperty(ref photoSource, value); }
        }

        public ICommand TakePhotoCommand { get; set; }

        private PhotoViewModel photoViewModel;

        private async void TakePhotoAsync()
        {
            if (photoViewModel == null)
            {
                photoViewModel = new PhotoViewModel();
            }
            var dialogResult = await dialogService.ShowDialogAsync(photoViewModel, this);
            if (dialogResult == true)
            {
                PhotoSource = photoViewModel.SnapshotTaken;
                photoViewModel.SnapshotBitmap = null;
                photoViewModel.SnapshotTaken = null;
            }
        }

        #endregion

        #region User-related

        private bool isUserSynced;

        public bool IsUserSynced
        {
            get { return isUserSynced; }
            private set
            {
                if (SetProperty(ref isUserSynced, value))
                {
                    OnPropertyChanged(() => ConfirmButtonText);
                }
            }
        }

        public ICommand SearchAndSyncUserCommand { get; private set; }

        private async void SearchAndSyncUserAsync()
        {
            if (string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(FirstName))
            {
                FailureMediator.Activate("Укажите имя и фамилию для поиска пользователя", true);
                return;
            }
            var searchPattern = string.Format("{0} {1} {2}", LastName, FirstName, MiddleName);
            try
            {
                BusyMediator.Activate("Поиск пользователя в Active Directory");
                var users = await userProvider.SearchUsersAsync(searchPattern);
                FoundUsers = users;
                if (users != null && users.Count == 1)
                {
                    SyncUser(users.First());
                }
                else
                {
                    ShowActiveDirectoryUserList = true;
                }
            }
            catch (PrincipalServerDownException ex)
            {
                FailureMediator.Activate("Не удалось установить связь с Active Directory. Пожалуйста, обратитесь в службу поддержки", null, ex, true);
            }
            catch (Exception ex)
            {
                FailureMediator.Activate("Не удалось найти пользователя в Active Directory", null, ex, true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private UserInfo currentUser;

        public UserInfo CurrentUser
        {
            get { return currentUser; }
            private set
            {
                if (SetTrackedProperty(ref currentUser, value))
                {
                    IsUserSynced = currentUser != null;
                }
            }
        }

        private IEnumerable<UserInfo> foundUsers;

        public IEnumerable<UserInfo> FoundUsers
        {
            get { return foundUsers; }
            private set
            {
                if (SetProperty(ref foundUsers, value))
                {
                    NoUserFound = foundUsers == null || !foundUsers.Any();
                }
            }
        }

        private bool noUserFound;

        public bool NoUserFound
        {
            get { return noUserFound; }
            private set { SetProperty(ref noUserFound, value); }
        }

        private bool showActiveDirectoryUserList;

        public bool ShowActiveDirectoryUserList
        {
            get { return showActiveDirectoryUserList; }
            set { SetProperty(ref showActiveDirectoryUserList, value); }
        }

        public ICommand SyncUserCommand { get; private set; }

        private void SyncUser(UserInfo user)
        {
            CurrentUser = user;
            ShowActiveDirectoryUserList = false;
        }

        public ICommand UnsyncUserCommand { get; private set; }

        private void UnsyncUser()
        {
            CurrentUser = null;
        }

        #endregion

        private DateTime? currentNameStartDate;

        private int currentPersonId;

        public int CurrentPersonId
        {
            get { return currentPersonId; }
            set
            {
                if (!SetProperty(ref currentPersonId, value))
                {
                    return;
                }
                if (currentPersonId.IsNewOrNonExisting())
                {
                    ClearData();
                }
                else
                {
                    LoadPatientInfoAsync();
                }
            }
        }

        public BusyMediator BusyMediator { get; private set; }

        public FailureMediator FailureMediator { get; private set; }

        public SaveUserInput PrepareDataForSave()
        {
            if (!ChangeTracker.HasChanges && !currentPersonId.IsNewOrNonExisting())
            {
                return null;
            }
            return new SaveUserInput
                   {
                       PersonId = currentPersonId,
                       LastName = currentPersonId.IsNewOrNonExisting() || ChangeTracker.PropertyHasChanges(() => LastName) ? LastName : null,
                       FirstName = currentPersonId.IsNewOrNonExisting() || ChangeTracker.PropertyHasChanges(() => FirstName) ? FirstName : null,
                       MiddleName = currentPersonId.IsNewOrNonExisting() || ChangeTracker.PropertyHasChanges(() => MiddleName) ? MiddleName : null,
                       IsMale = currentPersonId.IsNewOrNonExisting() || ChangeTracker.PropertyHasChanges(() => IsMale) ? IsMale : (bool?)null,
                       BirthDate = currentPersonId.IsNewOrNonExisting() || ChangeTracker.PropertyHasChanges(() => BirthDate) ? BirthDate : null,
                       Photo = currentPersonId.IsNewOrNonExisting() || ChangeTracker.PropertyHasChanges(() => PhotoSource) ? new ValueOf<byte[]>(fileService.GetBinaryDataFromImage(new JpegBitmapEncoder(), PhotoSource)) : ValueOf<byte[]>.Empty,
                       UserInfo = currentPersonId.IsNewOrNonExisting() || ChangeTracker.PropertyHasChanges(() => CurrentUser) ? new ValueOf<UserInfo>(CurrentUser) : ValueOf<UserInfo>.Empty,
                       NewNameStartDate = IsNewName ? NewNameStartDate : null
                   };
        }

        private readonly CommandWrapper loadPatientInfoCommandWrapper;

        private async void LoadPatientInfoAsync()
        {
            try
            {
                FailureMediator.Deactivate();
                BusyMediator.Activate("Загрузка данных пользователя...");
                var person = await userAccessService.GetPersonAsync(currentPersonId);
                var currentName = person.PersonNames.First(x => x.EndDateTime == SpecialValues.MaxDate);
                currentNameStartDate = currentName.BeginDateTime;
                LastName = currentName.LastName;
                FirstName = currentName.FirstName;
                MiddleName = currentName.MiddleName;
                BirthDate = person.BirthDate;
                IsMale = person.IsMale;
                PhotoSource = person.Document == null ? null : fileService.GetImageSourceFromBinaryData(person.Document.FileData);
                var user = person.Users.FirstOrDefault();
                CurrentUser = user == null ? null : new UserInfo
                                                    {
                                                        FullName = user.SystemName,
                                                        Login = user.Login,
                                                        Sid = user.SID
                                                    };
                ChangeTracker.IsEnabled = true;
            }
            catch (Exception ex)
            {
                FailureMediator.Activate("Не удалось загрузить данные пользователя. Попробуйте еще раз. Если ошибка повторится, пожалуйста, обратитесь в службу поддержки",
                                         loadPatientInfoCommandWrapper,
                                         ex);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private void ClearData()
        {
            ChangeTracker.IsEnabled = false;
            currentPersonId = SpecialValues.NonExistingId;
            currentNameStartDate = null;
            lastName = string.Empty;
            firstName = string.Empty;
            middleName = string.Empty;
            birthDate = null;
            photoSource = null;
        }

        public string Title
        {
            get { return string.Empty; }
        }

        public string ConfirmButtonText
        {
            get { return "Сохранить"; }
        }

        public string CancelButtonText
        {
            get { return "Отменить"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        private void Close(bool? validate)
        {
            if (validate == true)
            {
                if (Validate())
                {
                    OnCloseRequested(true);
                }
            }
            else
            {
                OnCloseRequested(false);
            }
        }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(bool confirmed)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, new ReturnEventArgs<bool>(confirmed));
            }
        }

        public IChangeTracker ChangeTracker { get; private set; }

        public void Dispose()
        {
            ChangeTracker.PropertyChanged -= OnChangesTracked;
        }

        #region Validation

        private readonly ValidationMediator<UserPropertiesDialogViewModel> validator; 

        private class Validator : ValidationMediator<UserPropertiesDialogViewModel>
        {
            public Validator(UserPropertiesDialogViewModel associatedItem)
                : base(associatedItem)
            {
            }

            protected override void OnValidateProperty(string propertyName)
            {
                if (PropertyNameEquals(propertyName, x => x.LastName))
                {
                    ValidateLastName();
                }
                else if (PropertyNameEquals(propertyName, x => x.FirstName))
                {
                    ValidateFirstName();
                }
                else if (PropertyNameEquals(propertyName, x => x.BirthDate))
                {
                    ValidateBirthDate();
                }
                else if (PropertyNameEquals(propertyName, x => x.IsIncorrectName) || PropertyNameEquals(propertyName, x => x.IsNewName))
                {
                    ValidateIsNewOrIncorrectName();
                }
                else if (PropertyNameEquals(propertyName, x => x.NewNameStartDate))
                {
                    ValidateNewNameStartDate();
                }
                else if (PropertyNameEquals(propertyName, x => x.CurrentUser))
                {
                    ValidateCurrentUser();
                }
            }

            protected override void RaiseAssociatedObjectPropertyChanged()
            {
                AssociatedItem.OnPropertyChanged(string.Empty);
            }

            protected override void OnValidate()
            {
                ValidateLastName();
                ValidateFirstName();
                ValidateBirthDate();
                ValidateIsNewOrIncorrectName();
                ValidateNewNameStartDate();
                ValidateCurrentUser();
            }

            private void ValidateLastName()
            {
                SetError(x => x.LastName, string.IsNullOrWhiteSpace(AssociatedItem.LastName) ? "Фамилия не может быть пустой" : string.Empty);
            }

            private void ValidateFirstName()
            {
                SetError(x => x.FirstName, string.IsNullOrWhiteSpace(AssociatedItem.FirstName) ? "Имя не может быть пустым" : string.Empty);
            }

            private void ValidateBirthDate()
            {
                var error = string.Empty;
                if (AssociatedItem.BirthDate == null)
                {
                    error = "Не выбрана дата рождения";
                }
                else if (AssociatedItem.BirthDate > DateTime.Today)
                {
                    error = "Дата рождения не может быть в будущем";
                }
                SetError(x => x.BirthDate, error);
            }

            private void ValidateIsNewOrIncorrectName()
            {
                var error = AssociatedItem.IsNameChanged && !AssociatedItem.IsNewName && !AssociatedItem.IsIncorrectName ? "Выберите причину смены Ф.И.О." : string.Empty;
                SetError(x => x.IsNewName, error);
                SetError(x => x.IsIncorrectName, error);
            }

            private void ValidateNewNameStartDate()
            {
                var error = string.Empty;
                if (AssociatedItem.IsNewName)
                {
                    if (AssociatedItem.NewNameStartDate == null)
                    {
                        error = "Укажите дату, с которой вступили силу изменения Ф.И.О.";
                    }
                    else if (AssociatedItem.NewNameStartDate < AssociatedItem.BirthDate)
                    {
                        error = "Дата смены Ф.И.О. не может быть меньше даты рождения";
                    }
                    else if (AssociatedItem.currentNameStartDate != null && AssociatedItem.NewNameStartDate < AssociatedItem.currentNameStartDate.Value)
                    {
                        error = string.Format("Дата смены Ф.И.О. не может быть меньше предыдущей даты смены Ф.И.О. ({0})",
                                              AssociatedItem.currentNameStartDate.Value.ToString(DateTimeFormats.ShortDateFormat));
                    }
                }
                SetError(x => x.NewNameStartDate, error);
            }

            private void ValidateCurrentUser()
            {
                var error = AssociatedItem.CurrentUser == null ? "Пользователь не привязан к Active Directory" : string.Empty;
                if (string.IsNullOrEmpty(error))
                {
                    error = AssociatedItem.cacheService.GetItems<User>().Any(x => x.SID == AssociatedItem.CurrentUser.Sid && x.PersonId != AssociatedItem.CurrentPersonId)
                        ? "В базе данных уже существует пользователь, привязанный к этой записи в Active Directory"
                        : string.Empty;
                }
                SetError(x => x.CurrentUser, error);
            }
        }

        public string this[string columnName]
        {
            get { return validator[columnName]; }
        }

        public string Error
        {
            get { return validator.Error; }
        }

        public bool Validate()
        {
            return ((IActiveDataErrorInfo)validator).Validate();
        }

        public void CancelValidation()
        {
            ((IActiveDataErrorInfo)validator).CancelValidation();
        }

        #endregion

    }
}