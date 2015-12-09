using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Core.Wpf.Converters;

namespace PatientSearchModule.Converters
{
    public class PatientGenderToImageSourceConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var photoUri = values[0] as ImageSource;
            if (photoUri != null)
            {
                return photoUri;
            }
            var isMale = (bool)values[1];
            return new BitmapImage((Uri)GenderToImageConverter.Instance.Convert(isMale, targetType, parameter, culture));

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
