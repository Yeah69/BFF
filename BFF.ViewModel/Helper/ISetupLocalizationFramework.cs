using System.Globalization;

namespace BFF.ViewModel.Helper
{
    public interface ISetupLocalizationFramework
    {
        void With(CultureInfo cultureInfo);
    }
}
