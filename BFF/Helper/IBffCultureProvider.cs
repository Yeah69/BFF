using System.Globalization;

namespace BFF.Helper
{
    public interface IBffCultureProvider
    {
        CultureInfo LanguageCulture { get; set; }
        CultureInfo CurrencyCulture { get; set; }
        CultureInfo DateCulture { get; set; }
        bool DateLong { get; set; }
    }
}
