using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Data;

namespace TelegramBot
{
    public class Part1
    {
        public Part1(IApplicationServices applicationServices)
        {
            _applicationServices = applicationServices;
        }

        private IApplicationServices _applicationServices;

        public async Task CallbackHub(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var callbackData = update.CallbackQuery.Data;
            var type = callbackData.Split('-')[0];
            var applicantId = Convert.ToInt32(callbackData.Split('-')[1]);


            var find = await _applicationServices.FindApplicantById(applicantId);

            if(find.Status != "status_waiting")
            {
                await botClient.SendTextMessageAsync(
                   chatId: update.CallbackQuery.From.Id,
                   text: "Данная заявка уже была просмотрена!",
                   cancellationToken: cancellationToken);

                return;
            }

            find.Status = $"status_{type}";
            await _applicationServices.UpdateApplicant(find);

            var client = await _applicationServices.FindClientById(find.AuthorId);

            if (type == "ok")
            {
                client.UniqRoleName = "user";
                await _applicationServices.UpdateClient(client);

                await botClient.SendTextMessageAsync(
                   chatId: client.ChatId,
                   text: "Ваша заявка была просмотрена и было вынесено решение: приянто.",
                   cancellationToken: cancellationToken);
            }
            else if (type == "denied")
            {
                await botClient.SendTextMessageAsync(
                   chatId: client.ChatId,
                   text: "Ваша заявка была просмотрена и было вынесено решение: отказ. Попробуйте снова заполнить анкету:",
                   cancellationToken: cancellationToken);
            }
            else if (type == "block")
            {
                client.isBanned = true;
                await _applicationServices.UpdateClient(client);
            }

        }

        public async Task MessageHub(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var chatId = update.Message.Chat.Id;
            var userId = update.Message.From.Id;
            var messageText = update.Message?.Text;

            var find = await _applicationServices.FindClientById(userId);

            if (find == null) // non auth
            {
                var client = new Client()
                {
                    ChatId = chatId,
                    isBanned = false,
                    Name = update.Message.From.Username,
                    UniqRoleName = "applicant",
                    UserId = userId
                };

                await _applicationServices.AddClient(client);

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Ваш аккаунт не был найден в базе данных. Он был обработан и теперь вы авторизированы! Сообщением ниже предоставте вашу анкету.",
                    cancellationToken: cancellationToken);
            }
            else // send applicant
            {
                if (find.UniqRoleName == "user")
                {
                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "У вас есть доступ к пиву. Я не буду вас слушать :)",
                    cancellationToken: cancellationToken);

                    return;
                }

                if (!find.isBanned && find.UniqRoleName != "admin")
                {
                    var isSending = await _applicationServices.UserIsSendApplicant(find);

                    if (isSending)
                    {
                        await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Вы уже отправили анкету. Дождитесь, пока администрация её проверит!",
                        cancellationToken: cancellationToken);

                        return;
                    }

                    if (update.Message != null && update.Message.Photo != null && !String.IsNullOrEmpty(update.Message.Caption))
                    {
                        messageText = update.Message.Caption;
                        var applicant = new Applicant()
                        {
                            Status = "status_waiting",
                            AuthorId = userId,
                            FileId = update.Message.Photo.First().FileId,
                            Text = messageText
                        };

                        await _applicationServices.AddAplicant(applicant);

                        var admins = await _applicationServices.GetAllAdminsChatIds();

                        InlineKeyboardMarkup inlineKeyboard = new(new[]
                         {
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData(text: "Принять", callbackData: $"ok-{applicant.Id}"),
                                 InlineKeyboardButton.WithCallbackData(text: "Отказать", callbackData: $"denied-{applicant.Id}"),
                             },
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData(text: "Заблокировать", callbackData: $"block-{applicant.Id}")
                             }
                         });

                        foreach (var k in admins)
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: k,
                                text: "Новая анкета от пользователя:\n\n" + messageText,
                                cancellationToken: cancellationToken);

                            await botClient.SendPhotoAsync(
                                chatId: k,
                                replyMarkup: inlineKeyboard,
                                photo: update.Message.Photo.First().FileId
                            );
                        }

                        await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Ваша анкета успешно разослана администрации. Дождитесь решения администрации.",
                        cancellationToken: cancellationToken);
                    }
                }
            }
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                await CallbackHub(botClient, update, cancellationToken);
            }
            else if (update.Message != null && (update.Message!.Type == MessageType.Text || update.Message!.Type == MessageType.Photo))
            {
                await MessageHub(botClient, update, cancellationToken);
            }
        }
    }
}
