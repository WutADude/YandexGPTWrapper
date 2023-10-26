# Описание
**YandexGPTWrapper** - это библиотека для работы с языковой моделью Яндекса "[YandexGPT](https://ya.ru/gpt/2)", позволяет легко реализовать отправку текстовых сообщений и получение текстовых ответов от языковой модели.
>Библиотека написана на C# (.NET 7)

# Примеры кода
Всё довольно просто и понятно.
> Пример первый

```C#
using YandexGPTWrapper;

namespace YGPTWrapperTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            YaGPT yagpt = new YaGPT(); // Объявляем класс
            string answer = await yagpt.SendMessageAsync("Вопрос"); // Отправляем вопрос и получаем ответ в виде строки
            Console.WriteLine(answer); // Выводим ответ пользователю
        }
    }
}
```

> Пример второй

Поскольку класс реализует интерфейс IDisposable мы можем сделать так.
```C#
using YandexGPTWrapper;

namespace YGPTWrapperTest
{
    internal class Program
    {
        static CancellationTokenSource _TokenSource = new CancellationTokenSource();
        static CancellationToken _CancelToken = _TokenSource.Token;

        static async Task Main(string[] args)
        {
            using (YaGPT yagpt = new YaGPT(сancelationToken:_CancelToken)) // Объявляем класс и инициализируем в нём токен отмены.
            { 
                string answer = await yagpt.SendMessageAsync("Вопрос"); // Отправляем вопрос и получаем ответ в виде строки
                Console.WriteLine(answer); // Выводим ответ пользователю
            }
        }
    }
}
```
Можно также увидеть, что при объявлении класса мы можем передать токен отмены и в будущем отменять задачи при необходимости.

> Пример третий

Если вам нужно, можно также подписаться на события получения сообщения и отключения сокета.
```C#
using YandexGPTWrapper;

namespace YGPTWrapperTest
{
    internal class Program
    {
        static CancellationTokenSource _TokenSource = new CancellationTokenSource();
        static CancellationToken _CancelToken = _TokenSource.Token;

        static async Task Main(string[] args)
        {
            using (YaGPT yagpt = new YaGPT(сancelationToken:_CancelToken)) // Объявляем класс и инициализируем в нём токен отмены.
            {
                // Подписываемся на события получения ответа и закрытия сокета.
                yagpt.OnMessageReceived += OnMessageRecieved;
                yagpt.OnWSocketClose += OnWSocketClose;
                // Отправляем вопрос и получаем ответ в виде строки, далее выводим пользователю.
                string answer = await yagpt.SendMessageAsync("Вопрос"); 
                Console.WriteLine(answer); // Выводим ответ пользователю
            }
        }

        private static void OnMessageReceived(string? messageText) // При срабатывании события мы получаем ответ от языковой модели в формате JSON строки.
        {
            // Делаем что хотим
        }

        private static void OnWSocketClose(string? closeReason) // При срабатывании события мы получаем причину отключения от сокета.
        {
            // Делаем что хотим
        }
    }
}
```
***Важно отметить, что строка возвращаемая из события "OnMessageReceived" - это сериализованный JSON в формате строки и десериализовать его придётся вручную, в то время как стандартный метод "SendMessageAsync" сам всё десериализует и возвращает только ответ в виде строки!***

# Важно
Библиотека полностью бесплатная, я не нашёл условий пользования на странице самой языковой модели.

Также я не нашёл какого-либо API на странице языковой модели (На момент октября 2023), библиотека реализована при помощи небольшого реверс инжиниринга работы языковой модели, из этого следует, что в библиотеке могут быть баги т.к. языковая модель активно развивается и механизм работы может меняться, а я могу не успевать за ними или вовсе не работать над ней.
