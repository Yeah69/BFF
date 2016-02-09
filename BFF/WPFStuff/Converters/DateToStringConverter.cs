﻿using System;
using System.Globalization;
using System.Windows.Data;
using BFF.Helper;

namespace BFF.WPFStuff.Converters
{
    public class DateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime) value;
            return date.ToString(BffEnvironment.CultureProvider.DateLong ? 
                BffEnvironment.CultureProvider.DateCulture.DateTimeFormat.LongDatePattern : 
                BffEnvironment.CultureProvider.DateCulture.DateTimeFormat.ShortDatePattern, 
                BffEnvironment.CultureProvider.DateCulture.DateTimeFormat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string date = (string)value;
            return DateTime.Parse(date, BffEnvironment.CultureProvider.DateCulture.DateTimeFormat);
        }
    }
}