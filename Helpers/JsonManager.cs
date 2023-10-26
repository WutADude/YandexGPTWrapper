using System.Text.Json;

namespace YandexGPTWrapper.Helpers
{
    /// <summary>
    /// Класс для упрощения работы с Json вопросами и ответами языковой модели.
    /// </summary>
    internal static class JsonManager
    {

        /// <summary>
        /// Метод, который вощвращает сериализованный объект в Json, в формате строки.
        /// </summary>
        /// <param name="classToSerialize">Объект для сериализации.</param>
        /// <returns>Строка в виде сериализованного Json объекта</returns>
        ///  <exception cref="MissingFieldException">Исключение, котое появляется в случае если переданный объект = null.</exception>
        internal static string GetSerializedJson(object classToSerialize)
        {
            _ = classToSerialize ?? throw new MissingFieldException(nameof(classToSerialize));
            return JsonSerializer.Serialize(classToSerialize);
        }

        /// <summary>
        /// Кортеж для получения основных значений из ответа языковой модели.
        /// </summary>
        /// <param name="jsonToDeserialize">Строка в виде Json объекта для десериализации и обработки.</param>
        /// <returns>(string) item1 = ответ в виде строки от языковой модели.
        /// (bool) item2 = является ли сообщение законченным ответом.
        /// (int) item3 = prefetchTime - время ожидания запроса следующего запроса на продолжение.</returns>
        /// <exception cref="MissingFieldException">Исключение, которое появляется в случае если переданный Json объект = пустой строке или null.</exception>
        internal static (string? messageText, bool isEnd, int prefetchTime) GetResponseData(string jsonToDeserialize)
        {
            if (string.IsNullOrWhiteSpace(jsonToDeserialize))
                throw new MissingFieldException(nameof(jsonToDeserialize));
            JsonElement rootElement = JsonDocument.Parse(jsonToDeserialize).RootElement;
            if (IsGoAwayDirectivePresented(rootElement)) // Иногда языковая модель может возвращать "GoAway" директиву, которая говорит о конце общения, вызываем исключение для обработки.
                throw new JsonException("GoAway directive presented.");
            JsonElement payloadElement = rootElement.GetProperty("directive").GetProperty("payload");
            JsonElement directiveElement = GetDirectivesElement(payloadElement);
            var responseData = (messageText: GetResponseText(directiveElement, payloadElement),
                isEnd: GetIsEndBoolean(directiveElement),
                prefetchTime: GetPrefetchTime(directiveElement)
                );
            return responseData;
        }

        private static string? GetResponseText(JsonElement directiveElement, JsonElement payloadElement)
        {
            if (directiveElement.TryGetProperty("text", out JsonElement textProperty))
                return textProperty.GetString();
            if (payloadElement.TryGetProperty("response", out JsonElement responseProperty))
                return responseProperty.GetProperty("card").GetProperty("text").GetString();
            if (payloadElement.GetProperty("response").TryGetProperty("chat_dialog_update", out JsonElement chatDialogProperty))
                if (chatDialogProperty.GetArrayLength() > 0)
                    return GetCDMessageProperty(payloadElement).GetProperty("content").GetProperty("plain_response_text").GetString();
            return null;
        }


        private static bool GetIsEndBoolean(JsonElement directiveElement)
        {
            if (directiveElement.TryGetProperty("is_end", out JsonElement isEnd))
                return isEnd.GetBoolean();
            return true;
        }

        private static int GetPrefetchTime(JsonElement directiveElement)
        {
            if (directiveElement.TryGetProperty("prefetch_after_ms", out JsonElement prefetchTime))
                return prefetchTime.GetInt32();
            return 0;
        }

        private static JsonElement GetDirectivesElement(JsonElement payloadElement)
        {
            if (payloadElement.GetProperty("response").TryGetProperty("directives", out JsonElement directivesProperty))
                if (directivesProperty.GetArrayLength() > 0)
                    return directivesProperty.EnumerateArray().GetEnumerator().FirstOrDefault().GetProperty("payload");
            return new JsonElement();
        }

        private static JsonElement GetCDMessageProperty(JsonElement chatDialogProperty) => chatDialogProperty.EnumerateArray().GetEnumerator()
            .FirstOrDefault().GetProperty("add_message_request").GetProperty("messages").EnumerateArray().GetEnumerator().FirstOrDefault();

        private static bool IsGoAwayDirectivePresented(JsonElement rootElement) => string.Compare(rootElement.GetProperty("directive").GetProperty("header").GetProperty("name").GetString(), "goaway", true) == 0;
    }
}
