using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace WindowUI.StartWork
{
    /// <summary>
    /// Interaction logic for StartWorkView
    /// </summary>
    public partial class StartWorkView : UserControl
    {
        public StartWorkView()
        {
            InitializeComponent();
        }
    }

    public class TimerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (TimeSpan) value;

            string h = date.Hours < 10 ? "0" +date.Hours : date.Hours.ToString();
            string m = date.Minutes < 10 ? "0" + date.Minutes : date.Minutes.ToString();
            string s = date.Seconds < 10 ? "0" + date.Seconds: date.Seconds.ToString();
            return $"{h}:{m}:{s}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
