using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using BFF.Model.Native.Structure;

namespace BFF.WPFStuff.Converters
{
    public class SubElementToSumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<SubTransInc> list = (IEnumerable<SubTransInc>) value;
            return list?.Sum(element => element.Sum);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
