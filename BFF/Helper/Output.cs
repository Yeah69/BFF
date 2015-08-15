using System;
using System.Runtime.CompilerServices;

namespace BFF.Helper
{
    class Output
    {
        public static void WriteLine(string text, [CallerMemberName] string callerName = null)
        {
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.Write("{0}:", callerName);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine(" {0}", text);
        }

        public static void WriteYNABTransaction(string[] entries)
        {

        }
    }
}
