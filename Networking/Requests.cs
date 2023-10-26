namespace YandexGPTWrapper.Networking
{
    internal static class Requests
    {
        internal static async Task<string> GetHtmlDocument()
        {
            using (HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) })
            {
                var response = await client.GetAsync("https://ya.ru/alisa_davay_pridumaem?utm_source=landing");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
                else
                    return string.Empty;
            }
        }
    }
}
