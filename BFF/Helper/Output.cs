using System;
using System.Runtime.CompilerServices;

namespace BFF.Helper
{
    class Output
    {
        public static void WriteLine(string text, [CallerMemberName] string callerName = null)
        {
            Console.WriteLine("{0}: {1}", callerName, text);
        }
    }
}
