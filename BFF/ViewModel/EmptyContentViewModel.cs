using System.Globalization;
using BFF.Properties;

namespace BFF.ViewModel
{
    public class EmptyContentViewModel : SessionViewModelBase
    {
        public EmptyContentViewModel()
        {
            Settings.Default.Culture_SessionCurrency = Settings.Default.Culture_DefaultCurrency;
            Settings.Default.Culture_SessionDate = Settings.Default.Culture_DefaultDate;
            ManageCultures();
        }

        #region Overrides of SessionViewModelBase

        protected override CultureInfo CreateCustomCulture()
        {
            CultureInfo customCulture = CultureInfo.CreateSpecificCulture(Settings.Default.Culture_DefaultLanguage.Name);
            customCulture.NumberFormat = Settings.Default.Culture_DefaultCurrency.NumberFormat;
            customCulture.DateTimeFormat = Settings.Default.Culture_DefaultDate.DateTimeFormat;
            return customCulture;
        }

        protected override void SaveCultures()
        {
            Settings.Default.Save();
        }

        #endregion
    }
}
