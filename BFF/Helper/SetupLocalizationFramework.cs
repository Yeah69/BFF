using System.Globalization;
using BFF.Core.Helper;

namespace BFF.Helper
{
    class SetupLocalizationFramework : ISetupLocalizationFramework
    {
        public void With(CultureInfo cultureInfo)
        {
            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = cultureInfo;
        }
    }
}
