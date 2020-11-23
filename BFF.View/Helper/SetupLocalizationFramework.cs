using System.Globalization;
using BFF.ViewModel.Helper;
using WPFLocalizeExtension.Engine;

namespace BFF.View.Helper
{
    class SetupLocalizationFramework : ISetupLocalizationFramework
    {
        public void With(CultureInfo cultureInfo)
        {
            LocalizeDictionary.Instance.Culture = cultureInfo;
        }
    }
}
