using BFF.ViewModel.Helper;
using System.Globalization;
using WPFLocalizeExtension.Engine;

namespace BFF.View.Wpf.Helper
{
    class SetupLocalizationFramework : ISetupLocalizationFramework
    {
        public void With(CultureInfo cultureInfo)
        {
            LocalizeDictionary.Instance.Culture = cultureInfo;
        }
    }
}
