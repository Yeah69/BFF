using System.Globalization;
using System.Threading;
using BFF.WPFStuff;

namespace BFF.ViewModel
{
    public abstract class SessionViewModelBase : ObservableObject
    {
        protected abstract CultureInfo CreateCustomCulture();

        protected abstract void SaveCultures();

        public void ManageCultures()
        {
            CultureInfo customCulture = CreateCustomCulture();
            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = customCulture;
            Thread.CurrentThread.CurrentCulture = customCulture;
            Thread.CurrentThread.CurrentUICulture = customCulture;
            SaveCultures();
            Messenger.Default.Send(CutlureMessage.Refresh);
        }
    }
}
