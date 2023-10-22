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
            if (classToSerialize is null)
                throw new MissingFieldException(nameof(classToSerialize));
            return JsonSerializer.Serialize(classToSerialize);
        }

        /// <summary>
        /// Кортеж для получения основных значений из ответа языковой модели.
        /// </summary>
        /// <param name="jsonToDeserialize">Строка в виде Json объекта для десериализации и обработки.</param>
        /// <returns>(string) item1 = ответ в виде строки от языковой модели.
        /// (bool) item2 = является ли сообщение законченным ответом.</returns>
        /// <exception cref="MissingFieldException">Исключение, которое появляется в случае если переданный Json объект = пустой строке или null.</exception>
        internal static (string?, bool) GetResponseData(string jsonToDeserialize)
        {

            if (string.IsNullOrWhiteSpace(jsonToDeserialize))
                throw new MissingFieldException(nameof(jsonToDeserialize));
            string? requestId, textResponse;
            bool isEnd = false;
            JsonElement payloadElement = JsonDocument.Parse(jsonToDeserialize).RootElement.GetProperty("directive").GetProperty("payload");
            JsonElement directiveElement = GetDirectivesElement(payloadElement); // Some problems from there, "directive" property non exist
            textResponse = GetResponseText(directiveElement, payloadElement);
            isEnd = GetIsEndBoolean(directiveElement);
            return (textResponse, isEnd);
        }

        private static string? GetResponseText(JsonElement directiveElement, JsonElement payloadElement)
        {
            if (directiveElement.TryGetProperty("text", out JsonElement textProperty))
                return textProperty.GetString();
            if (payloadElement.TryGetProperty("response", out JsonElement responseProperty))
                return responseProperty.GetProperty("card").GetProperty("text").GetString();
            return null;
        }


        private static bool GetIsEndBoolean(JsonElement directiveElement)
        {
            if (directiveElement.TryGetProperty("is_end", out JsonElement isEnd))
                return isEnd.GetBoolean();
            return true;
        }

        private static JsonElement GetDirectivesElement(JsonElement payloadElement)
        {
            if (payloadElement.GetProperty("response").TryGetProperty("directives", out JsonElement directivesProperty))
                if (directivesProperty.GetArrayLength() > 0)
                    return directivesProperty.EnumerateArray().GetEnumerator().FirstOrDefault().GetProperty("payload");
            return new JsonElement();
        }
    }
}
