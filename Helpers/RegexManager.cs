using System.Text.RegularExpressions;

namespace YandexGPTWrapper.Helpers
{
    internal static class RegexManager
    {
        private static Regex _ActualAppVerPattern = new Regex("\"production\",version:\"(\\S+)\"", RegexOptions.Compiled);
        internal static string? GetActualAppVersion(string htmlDocument) => _ActualAppVerPattern.Match(htmlDocument).Groups[1].Value;
    }
}
