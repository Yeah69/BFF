using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
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
            if (typeof(Transfer) == (Type)value) return TransferCanvas;
            if (typeof(Transaction) == (Type)value) return TransactionCanvas;
            return typeof(Income) == (Type)value ? IncomeCanvas : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
