using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using BFF.Model.Native;

namespace BFF.WPFStuff.Converters
{
    public class TypeToCanvasConverter : IValueConverter
    {
        public Canvas TransactionCanvas { get; set; } = null;

        public Canvas IncomeCanvas { get; set; } = null;

        public Canvas TransferCanvas { get; set; } = null;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if ((string)value == "Transfer") return TransferCanvas;
            if (((string)value).EndsWith("Trans")) return TransactionCanvas;
            return ((string)value).EndsWith("Income") ? IncomeCanvas : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
