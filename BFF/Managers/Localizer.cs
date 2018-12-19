using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.Extensions;

namespace BFF.Managers
{
    class Localizer : ILocalizer, IOncePerApplication
    {
        public string Localize(string key) => key.Localize();

        public T Localize<T>(string key) => key.Localize<T>();
    }
}
