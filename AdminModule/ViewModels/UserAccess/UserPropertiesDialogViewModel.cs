﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using AdminModule.Services;
using Core.Data.Misc;
using Core.Extensions;
using Core.Misc;
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

        private readonly ILog log;

        private readonly IDialogServiceAsync dialogService;

        private readonly IFileService fileService;

        public UserPropertiesDialogViewModel(IUserAccessService userAccessService, ILog log, IDialogServiceAsync dialogService, IFileService fileService)
        {
            if (userAccessService == null)
            {
                throw new ArgumentNullException("userAccessService");
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
            this.userAccessService = userAccessService;
            this.log = log;
            this.dialogService = dialogService;
            this.fileService = fileService;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            validator = new Validator(this);
            CloseCommand = new DelegateCommand<bool?>(Close);
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

        private readonly CommandWrapper loadPatientInfoCommandWrapper;

        private async void LoadPatientInfoAsync()
        {
            try
            {
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

        private ValidationMediator<UserPropertiesDialogViewModel> validator; 

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