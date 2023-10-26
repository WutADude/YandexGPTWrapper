using System.Text;
using System.Text.Json;
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
        /// (ПОДДЕРЖИВАЕТСЯ ТОЛЬКО РУССКИЙ ЯЗЫК, Я НЕ ЗНАЮ ЗАЧЕМ ДОБАВИЛ В БИБЛИОТЕКУ АНГЛИЙСКИЙ, возможно на будущее?)</param>
        /// <param name="cancelationToken">Токен для отмены операций.</param>
        public YaGPT(string language = Language.Russian, CancellationToken? cancelationToken = null) : base(cancelationToken)
        {
            _EventObjects = new EventObjects(language, Task.Run(async () => RegexManager.GetActualAppVersion(await Requests.GetHtmlDocument())).Result);
        }

        /// <summary>
        /// Отправляет сериализованный Json объект с сообщением и возвращает ответ в виде прямого ответа - строки без необходимости десериализации овтета вручную.
        /// Учитывает продолжение получения ответа если языковая модель не смогла уместить его в одно сообщение.
        /// </summary>
        /// <param name="message">Текст/вопрос для отправки.</param>
        /// <returns>Возвращает строку с ответом с учётом форматирования самой языковой модели.</returns>
        public async Task<string> SendMessageAsync(string? message)
        {
            StringBuilder answerString = new StringBuilder();
            try
            {
                var responseData = JsonManager.GetResponseData(await SendTextAsync(JsonManager.GetSerializedJson(_EventObjects.TextInputEvent(message))));
                answerString.Append(responseData.messageText);
                string? firstContinuationRequestId = null;
                while (!responseData.isEnd)
                {
                    if (responseData.prefetchTime > 0)
                        await Task.Delay(responseData.Item3, _CancelationToken ?? CancellationToken.None);
                    responseData = JsonManager.GetResponseData(await SendTextAsync(JsonManager.GetSerializedJson(_EventObjects.ContinuationEvent(ref firstContinuationRequestId))));
                    answerString.Append(responseData.Item1);
                }
                return answerString.ToString();
            }
            catch (JsonException)
            {
                await ReconnectToSocket();
                return await SendMessageAsync(message);
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}