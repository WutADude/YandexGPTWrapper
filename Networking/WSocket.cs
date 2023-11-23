using System.Text;
using System.Net.WebSockets;

namespace YandexGPTWrapper.Networking
{
    public abstract class WSocket : IDisposable
    {
        protected ClientWebSocket _WebSocket = new ClientWebSocket();
        public readonly CancellationToken? _CancelationToken;
        private readonly Uri _WSUri = new Uri("wss://uniproxy.alice.ya.ru/uni.ws");
        protected bool _Disposed = false;

        /// <summary>
        /// Конструктор класса для упрощения работы с WebSocket'ами при помощи System.Net.WebSockets.
        /// </summary>
        /// <param name="cancelationToken">Необязательный параметр, токен отмены операций.</param>
        public WSocket(CancellationToken? cancelationToken = null)
        {
            _CancelationToken = cancelationToken;
            Task.Run(async() => await ConnectToSocket()).Wait();
        }

        /// <summary>
        /// Подключение к сокету языковой модели.
        /// </summary>
        private async Task ConnectToSocket() => await _WebSocket.ConnectAsync(_WSUri, _CancelationToken ?? CancellationToken.None);

        /// <summary>
        /// Отправка текстового сообщения в языковую модель.
        /// </summary>
        /// <param name="message">Текст сообщения сериализованный в Json объект.</param>
        /// <param name="endMessage">Необязательный параметр отвечающий за то, является ли сообщение - окончанием, по стандарту = true.</param>
        /// <param name="waitResponse">Необязательный параметр отвечающий за то, нужно-ли нам ожидать ответа от языковой модели или нет, по стандарту = true.</param>
        /// <returns>Возвращает сериализованный ответ языковой модели в формате строки для последующей десериализации.</returns>
        protected async Task<string> SendEventAsync(string message, bool endMessage = true, bool waitResponse = true)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await _WebSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, endMessage, _CancelationToken ?? CancellationToken.None);
            if (!waitResponse)
                return string.Empty;
            byte[] incomingData = new byte[65536]; // Размер ответа всегда разный, выделенный буфер - это буфер с запасом.
            WebSocketReceiveResult receivedData = await _WebSocket.ReceiveAsync(new ArraySegment<byte>(incomingData), _CancelationToken ?? CancellationToken.None);
            if (receivedData.CloseStatus.HasValue)
            {
                OnWSocketClose?.Invoke(receivedData.CloseStatusDescription);
                return string.Empty;
            }
            if (receivedData.MessageType == WebSocketMessageType.Text)
            {
                string receivedMessage = Encoding.UTF8.GetString(incomingData, 0, receivedData.Count);
                OnMessageReceived?.Invoke(receivedMessage);
                return receivedMessage;
            }
            return string.Empty;
        }

        /// <summary>
        /// Задача переподключения к веб-сокету в случае "GoAway" - директивы.
        /// </summary>
        protected async Task ReconnectToSocket()
        {
            await _WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", _CancelationToken ?? CancellationToken.None);
            _WebSocket.Dispose();
            _WebSocket = new ClientWebSocket();
            await ConnectToSocket();
        }

        // Делегаты для создания событий/ивентов.
        public delegate void WSocketCloseHandler(string? closeDescription);
        public delegate void WSocketOnMessageReceived(string? messageText);

        /// <summary>
        /// Событие/ивент, возникающее при возврате сообщения о закрытии соединения.
        /// </summary>
        /// <param name="closeDescription">Возвращает причину закрытия в формате строки.</param>
        public event WSocketCloseHandler? OnWSocketClose;

        /// <summary>
        /// Событие/ивент, возникающее при получении какого-либо ответа от языковой модели.
        /// </summary>
        public event WSocketOnMessageReceived? OnMessageReceived;

        protected virtual async void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    await _WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing websocket.", CancellationToken.None);
                    _WebSocket?.Dispose();
                }
            }
            _Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
