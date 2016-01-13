using System;
using System.Windows.Navigation;
using Core.Data;
using Core.Misc;
using Core.Services;
using Core.Wpf.Mvvm;
using Prism.Commands;
using Prism.Mvvm;

namespace AdminModule.ViewModels
{
    public class GroupEditDialogViewModel : BindableBase, IDialogViewModel, IActiveDataErrorInfo
    {
        private readonly ICacheService cacheService;

        private readonly ValidationMediator<GroupEditDialogViewModel> validator;

        public GroupEditDialogViewModel(ICacheService cacheService)
        {
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.cacheService = cacheService;
            validator = new Validator(this);
            CloseCommand = new DelegateCommand<bool?>(Close);
        }

        public PermissionGroupViewModel ExistingPermissionGroup
        {
            get { return existingPermissionGroup; }
            set
            {
                existingPermissionGroup = value;
                Name = value == null ? string.Empty : value.Name;
                Description = value == null ? string.Empty : value.Description;
            }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private string description;

        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value); }
        }

        private string title;
        private PermissionGroupViewModel existingPermissionGroup;

        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public string ConfirmButtonText { get { return "Сохранить"; } }

        public string CancelButtonText { get { return "Отменить"; } }

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

        protected virtual void OnCloseRequested(bool validate)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, new ReturnEventArgs<bool>(validate));
            }
        }

        private class Validator : ValidationMediator<GroupEditDialogViewModel>
        {
            public Validator(GroupEditDialogViewModel associatedItem)
                : base(associatedItem)
            {
            }

            protected override void OnValidateProperty(string propertyName)
            {
                if (PropertyNameEquals(propertyName, x => x.Name))
                {
                    ValidateName();
                }
            }

            private void ValidateName()
            {
                var error = string.IsNullOrWhiteSpace(AssociatedItem.Name) ? "Не указана дата выдачи" : string.Empty;
                if (string.IsNullOrEmpty(error))
                {
                    var groupWithTheSameName = AssociatedItem.cacheService.GetItemByName<PermissionGroup>(AssociatedItem.Name.Trim());
                    error = groupWithTheSameName == null || (AssociatedItem.ExistingPermissionGroup != null && groupWithTheSameName == AssociatedItem.ExistingPermissionGroup.Group)
                        ? string.Empty
                        : "Группа с таким именем уже существует";
                }
                SetError(x => x.Name, error);
            }

            protected override void RaiseAssociatedObjectPropertyChanged()
            {
                AssociatedItem.OnPropertyChanged(string.Empty);
            }

            protected override void OnValidate()
            {
                ValidateName();
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
    }
}
