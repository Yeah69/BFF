using WPFLocalizeExtension.Extensions;

namespace BFF.View.Wpf.Extensions
{
    internal static class StringExtensions
    {
        public static T Localize<T>(this string key) =>
            LocExtension.GetLocalizedValue<T>("BFF.View.Wpf:Texts:" + key);

        public static string Localize(this string key) =>
            LocExtension.GetLocalizedValue<string>("BFF.View.Wpf:Texts:" + key);
    }
}
