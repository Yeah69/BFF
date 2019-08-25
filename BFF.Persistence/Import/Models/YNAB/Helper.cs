using System.Linq;

namespace BFF.Persistence.Import.Models.YNAB
{
    internal class Helper
    {
        internal static long ExtractLong(string text)
        {
            var number = text.ToCharArray().Where(c => char.IsDigit(c) || c == '-').Aggregate("", (current, character) => $"{current}{character}");
            return number == "" ? 0L : long.Parse(number);
        }
    }
}
