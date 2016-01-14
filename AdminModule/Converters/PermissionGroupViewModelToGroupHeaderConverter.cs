using System;
using System.Globalization;
using System.Windows.Data;
using AdminModule.ViewModels;

namespace AdminModule.Converters
{
    public class PermissionGroupViewModelToGroupHeaderConverter : IValueConverter
    {
        public static readonly PermissionGroupViewModelToGroupHeaderConverter Instance = new PermissionGroupViewModelToGroupHeaderConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var permissionGroup = (PermissionGroupViewModel)value;
            if (permissionGroup.IsInPermissionMode)
            {
                return permissionGroup.CurrentPermissionIsIncluded ? "Группы, содержащие право" : "Группы, не содержащие право";
            }
            if (permissionGroup.IsInUserMode)
            {
                return permissionGroup.CurrentUserIsIncluded ? "Пользователь входит в группы" : "Пользователь не входит в группы";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
