﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace BFF.WPFStuff.Converters
{
    public class DateComboConverter : IValueConverter
    {
        public static readonly DateTime SampleValue = new DateTime(2013, 9, 6);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var itemCulture = CultureInfo.GetCultureInfo((string) value);
            return (string) parameter == "long" ? SampleValue.ToString(itemCulture.DateTimeFormat.LongDatePattern) : SampleValue.ToString(itemCulture.DateTimeFormat.ShortDatePattern);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
           throw new NotImplementedException();
        }
    }
}
