using BFF.Core.IoC;
using BFF.View.Wpf.Extensions;
using BFF.ViewModel.Helper;

namespace BFF.View.Wpf.Managers
{
    class Localizer : ILocalizer, IOncePerApplication
    {
        public string Localize(string key) => key.Localize();

        public T Localize<T>(string key) => key.Localize<T>();
    }
}
