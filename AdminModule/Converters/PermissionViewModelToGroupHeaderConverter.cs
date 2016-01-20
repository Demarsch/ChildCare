using System;
using System.Globalization;
using System.Windows.Data;
using AdminModule.ViewModels;

namespace AdminModule.Converters
{
    public class PermissionViewModelToGroupHeaderConverter : IValueConverter
    {
        public static readonly PermissionViewModelToGroupHeaderConverter Instance = new PermissionViewModelToGroupHeaderConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var permission = (PermissionViewModel)value;
            if (permission.IsInUserMode)
            {
                return permission.IsOwnedByCurrentUser ? "Права, которые есть у пользователя" : "Права, которых у пользователя нет";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
