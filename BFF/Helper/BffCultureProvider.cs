using System.Globalization;

namespace BFF.Helper
{
    internal class BffCultureProvider : IBffCultureProvider
    {
        public CultureInfo LanguageCulture { get; set; } = CultureInfo.GetCultureInfo("en-US");
        public CultureInfo CurrencyCulture { get; set; } = CultureInfo.GetCultureInfo("de-DE");
        public CultureInfo DateCulture { get; set; } =  CultureInfo.GetCultureInfo("de-DE");

        public BffCultureProvider(CultureInfo languageCulture = null, CultureInfo currencyCulture = null,
            CultureInfo dateCulture = null)
        {
            LanguageCulture = languageCulture ?? LanguageCulture;
            CurrencyCulture = currencyCulture ?? CurrencyCulture;
            DateCulture = dateCulture ?? DateCulture;
        }
    }
}
