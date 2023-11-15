namespace YandexGPTWrapper.Networking
{
    internal static class Requests
    {
        /// <summary>
        /// Делаем запрос на главную страницу языковой модели для получения актуальной версии.
        /// </summary>
        /// <returns>HTML документ для дальнешей работы.</returns>
        internal static async Task<string> GetHtmlDocument()
        {
            using (HttpClient client = new HttpClient(new HttpClientHandler() { UseCookies = false }) { Timeout = TimeSpan.FromSeconds(5) })
            {
                var response = await client.GetAsync("https://ya.ru/alisa_davay_pridumaem?utm_source=landing");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
                return string.Empty;
            }
        }
    }
}
