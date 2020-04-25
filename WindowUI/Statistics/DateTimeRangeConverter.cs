using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace WindowUI.Statistics
{
    class DateTimeRangeConverter : IValueConverter
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _stringBuilder.Clear();

            var from = System.Convert.ToBoolean(parameter);
            var date = (DateTime) value;

            if (from)
            {
                _stringBuilder.Append("From: ");
            }
            else
            {
                _stringBuilder.Append("To: ");
            }

            _stringBuilder.Append(date.ToString("D"));
            return _stringBuilder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
