using System.Reflection;
using WPFLocalizeExtension.Extensions;

namespace BFF.Helper.Extensions
{
    internal static class StringExtensions
    {
        public static T Localize<T>(this string key) =>
            LocExtension.GetLocalizedValue<T>(Assembly.GetCallingAssembly().GetName().Name + ":Resources:" + key);
    }
}
