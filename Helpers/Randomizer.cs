namespace YandexGPTWrapper.Helpers
{
    /// <summary>
    /// Класс для упрощения некоторых моментов с генерацией рандомных ID.
    /// </summary>
    internal static class Randomizer
    {
        private static readonly string _AllChars = "abcdefghijklmnopqrstuvwxyz0123456789";
        private static readonly string _NumChars = "0123456789";

        /// <summary>
        /// Генерация рандомного ID в формате "********-****-****-****-************", где "*" - рандомная цифра/буква нижнего регстра.
        /// </summary>
        internal static string GetRandomId 
        {
            get
            {
                Random rnd = new Random();
                return new string(Enumerable.Repeat(_AllChars, 8).Select(s => s[rnd.Next(s.Length)]).ToArray()) + "-" + new string(Enumerable.Repeat(_AllChars, 4).Select(s => s[rnd.Next(s.Length)]).ToArray()) + "-" + new string(Enumerable.Repeat(_AllChars, 4).Select(s => s[rnd.Next(s.Length)]).ToArray()) + "-" + new string(Enumerable.Repeat(_AllChars, 4).Select(s => s[rnd.Next(s.Length)]).ToArray()) + "-" + new string(Enumerable.Repeat(_AllChars, 12).Select(s => s[rnd.Next(s.Length)]).ToArray());
            }
        }

        /// <summary>
        /// Генерация рандомного UUID в формате "0000000000000*******************", где "*" - рандомная цифра.
        /// </summary>
        internal static string GetRandomUUID
        {
            get
            {
                Random rnd = new Random(); // 00000000000004639405431630146925
                return $"0000000000000{new string(Enumerable.Repeat(_NumChars, 19).Select(s => s[rnd.Next(s.Length)]).ToArray())}";
            }
        }
    }
}
