using System.Text.RegularExpressions;

namespace YandexGPTWrapper.Helpers
{
    /// <summary>
    /// Класс для упрощения работы с регулярными выражениями.
    /// </summary>
    internal static class RegexManager
    {
        private static Regex _ActualAppVerPattern = new Regex("\"production\",version:\"(\\S+)\"", RegexOptions.Compiled);
        internal static string? GetActualAppVersion(string htmlDocument) => _ActualAppVerPattern.Match(htmlDocument).Groups[1].Value;
    }
}
