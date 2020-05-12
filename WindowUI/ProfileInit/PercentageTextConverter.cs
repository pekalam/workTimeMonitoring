using System;
using System.Globalization;
using System.Windows.Data;

namespace WindowUI.ProfileInit
{
    public class PercentageTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value).ToString() + " %";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
