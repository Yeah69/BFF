﻿using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace BFF.WPFStuff.Converters
{
    public class BooleanToDatePickerFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? DatePickerFormat.Long : DatePickerFormat.Short;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return (DatePickerFormat) value == DatePickerFormat.Long;
        }
    }
}
