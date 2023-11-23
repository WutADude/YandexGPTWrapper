using System.Text.Json.Nodes;
using YandexGPTWrapper.Helpers;

namespace YandexGPTWrapper.JObjects
{
    /// <summary>
    /// Класс для создания Json объектов, которые будут отправляться в языковую модель.
    /// </summary>
    internal sealed class EventObjects
    {
        private int _SeqNumber = 1;
        private readonly string _UUID, _WorkingLanguage;
        private readonly string _ActualAppVersion = "1.0.281-home-static/alice-web/15";
        private const string _DialogSkillId = "b7c42cab-db61-46ba-871a-b10a6ecf3e0d";
        private string? _LastRequestId;

        /// <summary>
        /// Конструктор класса для создания Json объектов.
        /// </summary>
        /// <param name="workingLanguage">Необязательный параметр, определяющий язык работы языковой модели.</param>
        /// <param name="appVersion">Актуальная версия языковой модели, парсится при инициализации классов автоматически.</param>
        internal EventObjects(string workingLanguage, string? appVersion)
        {
            _WorkingLanguage = workingLanguage;
            _UUID = Randomizer.GetRandomUUID;
            _ActualAppVersion = string.IsNullOrWhiteSpace(appVersion) ? _ActualAppVersion : appVersion;
        }

        /// <summary>
        /// Объект, который создаётся и отправляется в языковую модель для "авторизации", хотя она таковой и не является, но без неё нельзя получать продолжения ответов.
        /// </summary>
        internal JsonObject AuthEvent
        {
            get
            {
                JsonObject auth = new JsonObject()
                {
                    ["event"] = new JsonObject()
                    {
                        ["header"] = new JsonObject()
                        {
                            ["namespace"] = "System",
                            ["name"] = "SynchronizeState",
                            ["messageId"] = Randomizer.GetRandomId,
                            ["seqNumber"] = _SeqNumber
                        },
                        ["payload"] = new JsonObject()
                        {
                            ["auth_token"] = Randomizer.GetRandomId,
                            ["uuid"] = _UUID,
                            ["vins"] = new JsonObject()
                            {
                                ["application"] = new JsonObject()
                                {
                                    ["app_id"] = "ru.yandex.webdesktop",
                                    ["platform"] = "windows"
                                }
                            }
                        }
                    }
                };
                return auth;
            }
        }

        /// <summary>
        /// Объект, который создаётся и отправляется при отправке вопроса в языковую модель, на который нужно получить ответ.
        /// </summary>
        /// <param name="message">Вопрос для получения ответа.</param>
        /// <returns>Возвращает JsonObject для дальнейшей сериализации и отправки в формате строки.</returns>
        /// <exception cref="MissingFieldException">Исключение в случае если сообщение пустое.</exception>
        internal JsonObject TextInputEvent(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new MissingFieldException(nameof(message));
            JsonObject textInput = new JsonObject()
            {
                ["event"] = new JsonObject()
                {
                    ["header"] = new JsonObject()
                    {
                        ["namespace"] = "Vins",
                        ["name"] = "TextInput",
                        ["messageId"] = Randomizer.GetRandomId,
                        ["seqNumber"] = _SeqNumber
                    },
                    ["payload"] = new JsonObject()
                    {
                        ["application"] = new JsonObject()
                        {
                            ["app_id"] = "ru.yandex.webdesktop",
                            ["app_version"] = _ActualAppVersion,
                            ["platform"] = "windows",
                            ["os_version"] = "mozilla/5.0 (windows nt 10.0; win64; x64) applewebkit/537.36 (khtml, like gecko) chrome/116.0.0.0 safari/537.36 opr/102.0.0.0",
                            ["uuid"] = _UUID,
                            ["lang"] = _WorkingLanguage,
                            ["client_time"] = DateTime.Now.ToString("s").Replace("-", "").Replace(":", ""),
                            ["timezone"] = "Europe/Moscow",
                            ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                        },
                        ["header"] = new JsonObject()
                        {
                            ["prev_req_id"] = Randomizer.GetRandomId,
                            ["sequence_number"] = null,
                            ["request_id"] = _LastRequestId = Randomizer.GetRandomId,
                            ["dialog_id"] = _DialogSkillId,
                            ["dialog_type"] = 1
                        },
                        ["request"] = new JsonObject()
                        {
                            ["event"] = new JsonObject()
                            {
                                ["type"] = "text_input",
                                ["text"] = message
                            },
                            ["voice_session"] = false,
                            ["experiments"] = new JsonArray() { "set_symbols_per_second=200", "stroka_yabro", "search_use_cloud_ui", "weather_use_cloud_ui", "enable_open_link_and_cloud_ui", "hw_onboarding_enable_greetings", "remove_feedback_suggests", "shopping_list", "enable_external_skills_for_webdesktop_and_webtouch", "send_show_view_directive_on_supports_show_view_layer_content_interface", "use_app_host_pure_Dialogovo_scenario", "div2cards_in_external_skills_for_web_standalone" },
                            ["additional_options"] = new JsonObject()
                            {
                                ["bass_options"] = new JsonObject()
                                {
                                    ["screen_scale_factor"] = 1
                                },
                                ["supported_features"] = new JsonArray() { "open_link", "server_action", "cloud_ui", "cloud_first_screen_div", "cloud_ui_filling", "show_promo", "show_view_layer_content", "reminders_and_todos", "div2_cards", "print_text_in_message_view", "supports_print_text_in_message_view", "player_pause_directive", "supports_rich_json_cards_in_fullscreen_mode_in_skills" },
                                ["unsupported_features"] = new JsonArray()
                            }
                        },
                        ["format"] = "audio/ogg;codecs=opus",
                        ["mime"] = "audio/webm;codecs=opus",
                        ["topic"] = "desktopgeneral"
                    }
                }
            };
            Interlocked.Increment(ref _SeqNumber);
            return textInput;
        }

        /// <summary>
        /// Объект, который создаётся и отправляется в случае запроса продолжения ответа, если языковая модель не подтвердила окончание ответа.
        /// </summary>
        /// <param name="parrentRequestId">ID сообщения с первым запросом продолжения, для получения продолжения ответа.</param>
        /// <returns>Возвращает JsonObject для дальнейшей сериализации и отправки в формате строки.</returns>
        internal JsonObject ContinuationEvent(ref string? parrentRequestId)
        {
            string currRequestId = string.Empty;
            JsonObject continuation = new JsonObject()
            {
                ["event"] = new JsonObject()
                {
                    ["header"] = new JsonObject()
                    {
                        ["messageId"] = Randomizer.GetRandomId,
                        ["name"] = "TextInput",
                        ["namespace"] = "Vins",
                        ["seqNumber"] = _SeqNumber
                    },
                    ["payload"] = new JsonObject()
                    {
                        ["application"] = new JsonObject()
                        {
                            ["app_id"] = "ru.yandex.webdesktop",
                            ["app_version"] = _ActualAppVersion,
                            ["platform"] = "windows",
                            ["os_version"] = "mozilla/5.0 (windows nt 10.0; win64; x64) applewebkit/537.36 (khtml, like gecko) chrome/116.0.0.0 safari/537.36 opr/102.0.0.0",
                            ["uuid"] = _UUID,
                            ["lang"] = _WorkingLanguage,
                            ["client_time"] = DateTime.Now.ToString("s").Replace("-", "").Replace(":", ""),
                            ["timezone"] = "Europe/Moscow",
                            ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                        },
                        ["header"] = new JsonObject()
                        {
                            ["prev_req_id"] = _LastRequestId,
                            ["sequence_number"] = null,
                            ["request_id"] = currRequestId = Randomizer.GetRandomId,
                            ["dialog_id"] = _DialogSkillId,
                            ["dialog_type"] = 1
                        },
                        ["request"] = new JsonObject()
                        {
                            ["additional_options"] = new JsonObject()
                            {
                                ["bass_options"] = new JsonObject()
                                {
                                    ["screen_scale_factor"] = 1
                                },
                                ["supported_features"] = new JsonArray() { "open_link", "server_action", "cloud_ui", "cloud_first_screen_div", "cloud_ui_filling", "show_promo", "show_view_layer_content", "reminders_and_todos", "div2_cards", "print_text_in_message_view", "supports_print_text_in_message_view", "player_pause_directive", "supports_rich_json_cards_in_fullscreen_mode_in_skills" },
                                ["unsupported_features"] = new JsonArray()
                            },
                            ["event"] = new JsonObject()
                            {
                                ["type"] = "server_action",
                                ["name"] = "@@mm_stack_engine_get_next",
                                ["payload"] = new JsonObject()
                                {
                                    ["@recovery_params"] = new JsonObject(),
                                    ["@request_id"] = _LastRequestId,
                                    ["stack_session_id"] = parrentRequestId ??= _LastRequestId,
                                    ["@scenario_name"] = "Dialogovo",
                                    ["stack_product_scenario_name"] = "dialogovo"
                                }
                            },
                            ["experiments"] = new JsonArray() { "set_symbols_per_second=200", "stroka_yabro", "search_use_cloud_ui", "weather_use_cloud_ui", "enable_open_link_and_cloud_ui", "hw_onboarding_enable_greetings", "remove_feedback_suggests", "shopping_list", "enable_external_skills_for_webdesktop_and_webtouch", "send_show_view_directive_on_supports_show_view_layer_content_interface", "use_app_host_pure_Dialogovo_scenario", "div2cards_in_external_skills_for_web_standalone" },
                            ["voice_session"] = false
                        },
                        ["format"] = "audio/ogg;codecs=opus",
                        ["mime"] = "audio/webm;codecs=opus",
                        ["topic"] = "desktopgeneral"
                    }
                }
            };
            _LastRequestId = currRequestId;
            Interlocked.Increment(ref _SeqNumber);
            return continuation;
        }

        /// <summary>
        /// Возвращает ID последнего запроса оправленного пользователем.
        /// </summary>
        internal string? GetLastRequestId => _LastRequestId;

        /// <summary>
        /// Возвращает текущий ID самого пользователя. (По факту - бесполезен и генерируется рандомно)
        /// </summary>
        internal string GetUUID => _UUID;

        /// <summary>
        /// Возвращает текущий номер очерёдности, или же число отправленных пользователем запросов.
        /// </summary>
        internal int GetCurrentSequenseNumber => _SeqNumber;

        /// <summary>
        /// Возвращает текущую используемую версию языковой модели.
        /// </summary>
        internal string GetCurrentYaGPTVersion => _ActualAppVersion;
    }
}
