using System.Text;
using YandexGPTWrapper.Helpers;
using YandexGPTWrapper.JObjects;
using YandexGPTWrapper.Networking;

namespace YandexGPTWrapper
{
    public class YaGPT : WSocket, IDisposable
    {
        private readonly EventObjects _EventObjects;
        /// <summary>
        /// Конструктор класса для взаимодействия с языковой моделью.
        /// </summary>
        /// <param name="language">Необязательный параметр, который отвечает за то, на каком языке будет работать языковая модель. 
        /// (ПОДДЕРЖИВАЕТСЯ ТОЛЬКО РУССКИЙ ЯЗЫК, Я НЕ ЗНАЮ ЗАЧЕМ ДОБАВИЛ АНГЛИЙСКИЙ, возможно на будущее?)</param>
        /// <param name="cancelationToken">Токен для отмены операций.</param>
        public YaGPT(string language = Language.Russian, CancellationToken? cancelationToken = null) : base(cancelationToken)
        {
            _EventObjects = new EventObjects(language);
        }

        /// <summary>
        /// Отправляет сериализованный Json объект с сообщением и возвращает ответ в виде прямого ответа - строки без необходимости десериализации овтета вручную.
        /// Учитывает продолжение получения ответа если языковая модель не смогла уместить его в одно сообщение.
        /// </summary>
        /// <param name="message">Текст/вопрос для отправки.</param>
        /// <returns>Возвращает строку с ответом с учётом форматирования самой языковой модели.</returns>
        public async Task<string> SendMessageAsync(string? message)
        {
            StringBuilder finalString = new StringBuilder();
            string response = await SendTextAsync(JsonManager.GetSerializedJson(_EventObjects.TextInputEvent(message)));
            var tupleValues = JsonManager.GetResponseData(response);
            finalString.Append(tupleValues.Item1);
            string? firstContinuationRequestId = null;
            while (!tupleValues.Item2)
            {
                tupleValues = JsonManager.GetResponseData(await SendTextAsync(JsonManager.GetSerializedJson(_EventObjects.ContinuationEvent(ref firstContinuationRequestId))));
                finalString.Append(tupleValues.Item1);
            }
            return finalString.ToString();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}