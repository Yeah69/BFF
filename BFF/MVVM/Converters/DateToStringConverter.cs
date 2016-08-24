using System;
using System.Globalization;
using System.Windows.Data;
using BFF.Properties;

namespace BFF.MVVM.Converters
{
    public class DateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime) value;
            return date.ToString(Settings.Default.Culture_DefaultDateLong ?
                Settings.Default.Culture_SessionDate.DateTimeFormat.LongDatePattern :
                Settings.Default.Culture_SessionDate.DateTimeFormat.ShortDatePattern,
                Settings.Default.Culture_SessionDate.DateTimeFormat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string date = (string)value;
            return DateTime.Parse(date, Settings.Default.Culture_SessionDate.DateTimeFormat);
        }
    }
}
