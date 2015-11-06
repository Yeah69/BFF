using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace BFF.Helper
{
    static class Output
    {
        public static void WriteLine(string text, [CallerMemberName] string callerName = null)
        {
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.Write(@"{0}:", callerName);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine(@" {0}", text);
        }
        
        public static string AsCurrency(this long value)
        {
            return value.AsCurrency(CultureInfo.CurrentCulture);
        }

        public static string AsCurrency(this long value, CultureInfo culture)
        {
            decimal result = value / 100m;
            return result.ToString("C", culture);
        }

        public static void WriteYnabTransaction(string[] entries)
        {

        }
    }
}
