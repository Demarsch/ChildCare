using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Core.Wpf.Converters;

namespace PatientInfoModule.Converters
{
    public class PatientGenderToPhotoUriConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var photoUri = values[0] as string;
            if (!string.IsNullOrEmpty(photoUri))
            {
                return new BitmapImage(new Uri(photoUri));
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
