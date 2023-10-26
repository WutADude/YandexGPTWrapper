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
            JsonElement directiveElement = GetDirectiveElement(payloadElement);
            var responseData = (messageText: GetResponseText(directiveElement, payloadElement),
                isEnd: GetIsEndBoolean(directiveElement),
                prefetchTime: GetPrefetchTime(directiveElement)
                );
            return responseData;
        }

        /// <summary>
        /// Метод для получения прямого ответа из десериализованного Json объекта.
        /// </summary>
        /// <param name="directiveElement">Json объект директивы.</param>
        /// <param name="payloadElement">Json объект пейлоада.</param>
        /// <returns>Возвращает ответ от языковой модели в формате строки с учётом форматирования самой языковой моделью. (Будут сохранены все переносы, символы, ковычки и т.п.)</returns>
        private static string? GetResponseText(JsonElement directiveElement, JsonElement payloadElement)
        {
            if (directiveElement.ValueKind == JsonValueKind.Object && directiveElement.TryGetProperty("text", out JsonElement textProperty))
                return textProperty.GetString();
            if (payloadElement.TryGetProperty("response", out JsonElement responseProperty))
                return responseProperty.GetProperty("card").GetProperty("text").GetString();
            if (payloadElement.GetProperty("response").TryGetProperty("chat_dialog_update", out JsonElement chatDialogProperty))
                if (chatDialogProperty.GetArrayLength() > 0)
                    return GetCDMessageProperty(payloadElement).GetProperty("content").GetProperty("plain_response_text").GetString();
            return null;
        }

        /// <summary>
        /// Метод для получения булевого значения, означающего завершение ответа на заданный вопрос.
        /// </summary>
        /// <param name="directiveElement">Json объект директивы.</param>
        /// <returns>Возвращает true | false в зависимости от того, закончен ли ответ или нет.</returns>
        private static bool GetIsEndBoolean(JsonElement directiveElement)
        {
            if (directiveElement.ValueKind == JsonValueKind.Object && directiveElement.TryGetProperty("is_end", out JsonElement isEnd))
                return isEnd.GetBoolean();
            return true;
        }

        /// <summary>
        /// Метод для получения целочисленного значения, а точнее времени ожидания следующего запроса на продолжение в мс (милисекундах).
        /// </summary>
        /// <param name="directiveElement">Json объект директивы.</param>
        /// <returns>Возвращает целочисленное (int) значение - означающее время ожидания.</returns>
        private static int GetPrefetchTime(JsonElement directiveElement)
        {
            if (directiveElement.ValueKind == JsonValueKind.Object && directiveElement.TryGetProperty("prefetch_after_ms", out JsonElement prefetchTime))
                return prefetchTime.GetInt32();
            return 0;
        }

        /// <summary>
        /// Метод для получения Json объекта директивы.
        /// </summary>
        /// <param name="payloadElement">Json объект пейлоада.</param>
        /// <returns>Json объект директивы.</returns>
        private static JsonElement GetDirectiveElement(JsonElement payloadElement)
        {
            if (payloadElement.GetProperty("response").TryGetProperty("directives", out JsonElement directivesProperty))
                if (directivesProperty.GetArrayLength() > 0)
                    return directivesProperty.EnumerateArray().GetEnumerator().FirstOrDefault().GetProperty("payload");
            return new JsonElement();
        }

        /// <summary>
        /// Метод для получения Json объекта из массива "messages" для дальнейшей работы.
        /// </summary>
        /// <param name="chatDialogProperty">Json объект "chatDialog".</param>
        /// <returns>Возвращает первый элемент массива "messages".</returns>
        private static JsonElement GetCDMessageProperty(JsonElement chatDialogProperty) => chatDialogProperty.EnumerateArray().GetEnumerator()
            .FirstOrDefault().GetProperty("add_message_request").GetProperty("messages").EnumerateArray().GetEnumerator().FirstOrDefault();

        /// <summary>
        /// Метод для проверки на директиву "GoAway", которая возникает в некоторых случаях и отправляется самой языковой моделью, в случае представления - языковая модель перестаёт общение и нужно переподключаться к сокету.
        /// </summary>
        /// <param name="rootElement">Коренной Json объект.</param>
        /// <returns>Возвращает true | false в зависимости от того, была ли представленна эта директива или нет.</returns>
        private static bool IsGoAwayDirectivePresented(JsonElement rootElement) => string.Compare(rootElement.GetProperty("directive").GetProperty("header").GetProperty("name").GetString(), "goaway", true) == 0;
    }
}
