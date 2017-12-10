using System.Globalization;
using System.Threading;

namespace BFF.MVVM.ViewModels
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
            Messenger.Default.Send(CultureMessage.Refresh);
        }
    }
}
