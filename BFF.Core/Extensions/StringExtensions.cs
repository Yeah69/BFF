using System.IO;
using System.Text.RegularExpressions;

namespace BFF.Core.Extensions
{
    public static class StringExtensions
    {

        public static bool IsNullOrWhiteSpace(this string @this) => string.IsNullOrWhiteSpace(@this);

        public static bool IsNullOrEmpty(this string @this) => string.IsNullOrEmpty(@this);

        private static readonly Regex MatchAllIllegalFilePathCharacters = new Regex(
            $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()))}]");

        public static string RemoveIllegalFilePathCharacters(this string @this)
        {
            return MatchAllIllegalFilePathCharacters.Replace(@this, "");
        }
    }
}
