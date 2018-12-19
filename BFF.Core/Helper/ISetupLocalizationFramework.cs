using System.Globalization;

namespace BFF.Core.Helper
{
    public interface ISetupLocalizationFramework
    {
        void With(CultureInfo cultureInfo);
    }
}
