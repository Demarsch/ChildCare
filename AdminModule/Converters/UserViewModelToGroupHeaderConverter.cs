using System;
using System.Globalization;
using System.Windows.Data;
using AdminModule.ViewModels;

namespace AdminModule.Converters
{
    public class UserViewModelToGroupHeaderConverter : IValueConverter
    {
        public static readonly UserViewModelToGroupHeaderConverter Instance = new UserViewModelToGroupHeaderConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var permission = (UserViewModel)value;
            if (permission.IsInPermissionMode)
            {
                return permission.OwnsCurrentPermission ? "Пользователи, у которых есть это право" : "Пользователи, у которых нет этого права";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
