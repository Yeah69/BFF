using System.Reflection;
using WPFLocalizeExtension.Extensions;

namespace BFF.Extensions
{
    internal static class StringExtensions
    {
        public static T Localize<T>(this string key) =>
            LocExtension.GetLocalizedValue<T>(Assembly.GetCallingAssembly().GetName().Name + ":Resources:" + key);

        public static string Localize(this string key) =>
            LocExtension.GetLocalizedValue<string>(Assembly.GetCallingAssembly().GetName().Name + ":Resources:" + key);
    }
}
