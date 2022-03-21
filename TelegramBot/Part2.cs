using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Data;

namespace TelegramBot
{
    public class Part2
    {
        public Part2(IApplicationServices applicationServices)
        {
            _applicationServices = applicationServices;
        }

        private IApplicationServices _applicationServices;

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message == null) return;

            if (update.Message!.Type == MessageType.Text || update.Message!.Type == MessageType.Photo)
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
                        UniqRoleName = "artist",
                        UserId = userId
                    };

                    await _applicationServices.AddClient(client);

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Ваш аккаунт не был найден в базе данных. Он был обработан и теперь вы авторизированы! Сообщением ниже предоставте вашу анкету.",
                        cancellationToken: cancellationToken);
                }
                else
                {
                    if(messageText == "/help")
                    {
                        var text = "Доступные вам комманды бота:\n\n";

                        if(find.UniqRoleName == "artist")
                        {
                            text += "    /create - создать АРТ-объект.\n";
                            text += "    /deleteAll - удалить все АРТ-объекты.\n";
                        }

                        if(find.UniqRoleName == "admin")
                        {
                            text += "    /objects - вывести список всех объектов.\n";
                            text += "    /object [id] - посмотреть информацию об отдельном объекте.\n";
                            text += "    /editobject [id] [newAuthorName] [newDate] - изменить определённый объект.\n";
                        }

                        await botClient.SendTextMessageAsync(
                               chatId: chatId,
                               text: text,
                               cancellationToken: cancellationToken);
                    }
                    else if (messageText == "/create") // Создать объект
                    {
                        if(find.UniqRoleName == "artist")
                        {
                            var artobj = new ArtObject()
                            {
                                AuthorName = update.Message.From.Username,
                                Date = DateTime.Now.ToString("MM/dd/yyyy HH:mm")
                            };

                            await _applicationServices.CreateArtObject(artobj);

                            await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Объект {artobj:" + artobj.Id + "} был успешно создан!",
                            cancellationToken: cancellationToken);
                        }
                        else await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Вы не имеете доступ к этому действию!",
                        cancellationToken: cancellationToken);
                    }
                    else if (messageText == "/deleteAll") // Удалить все объекты
                    {
                        if (find.UniqRoleName == "artist")
                        {
                            await _applicationServices.DeleteAllObject();

                            await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Все АРТ объекты были успешно удаленны.",
                            cancellationToken: cancellationToken);
                        }
                        else await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Вы не имеете доступ к этому действию!",
                        cancellationToken: cancellationToken);
                    }
                    else if (messageText == "/objects") // посмотреть все объекты
                    {
                        if (find.UniqRoleName == "admin")
                        {
                            var list = await _applicationServices.GetAllArtObjects();
                            var message = "Список АРТ-объектов:\n\n";
                            var z = 1;

                            foreach(var obj in list)
                            {
                                message += $"{z}. [#{obj.Id}] - Объёкт авторством {obj.AuthorName}\nДата создания: {obj.Date}\n\n";
                                z++;
                            }

                            await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: message,
                           cancellationToken: cancellationToken);
                        }
                        else await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Вы не имеете доступ к этому действию!",
                        cancellationToken: cancellationToken);
                    }
                    else if (messageText.StartsWith("/object ")) // посмотреть один объект
                    {
                        try
                        {
                            var objectId = Convert.ToInt32(messageText.Replace("/object ", ""));

                            if (find.UniqRoleName == "admin")
                            {
                                var obj = await _applicationServices.GetArtObjectById(objectId);

                                if (obj != null)
                                {

                                    await botClient.SendTextMessageAsync(
                               chatId: chatId,
                               text: $"Найденный объёкт [#{obj.Id}]: авторство {obj.AuthorName}, дата создания: {obj.Date}.",
                               cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(
                               chatId: chatId,
                               text: $"Объект с индифактором [#{objectId}] не найден.",
                               cancellationToken: cancellationToken);
                                }
                            }
                            else await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Вы не имеете доступ к этому действию!",
                            cancellationToken: cancellationToken);
                        }
                        catch (Exception)
                        {
                            await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Неверный параметр id объекта!",
                            cancellationToken: cancellationToken);

                            return;
                        }
                    }
                    else if (messageText.StartsWith("/editobject ")) // начать изменение одного объекта
                    {
                        try
                        {

                            if (find.UniqRoleName == "admin")
                            {
                                var args = messageText.Split(' ');

                                if (args.Count() != 4)
                                {
                                    await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Неправильный ввод! Попробуйте: /editobject [id] [authorName] [Date]",
                                    cancellationToken: cancellationToken);

                                    return;
                                }

                                var obj = await _applicationServices.GetArtObjectById(Convert.ToInt32(args[1]));

                                if(obj != null)
                                {
                                    obj.AuthorName = args[2];
                                    obj.Date = args[3];

                                    await _applicationServices.UpdateArtObject(obj);

                                    await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Арт объект успешно обновлён!",
                                    cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(
                               chatId: chatId,
                               text: $"Объект с индифактором [#{args[1]}] не найден.",
                               cancellationToken: cancellationToken);
                                }

                            }
                            else await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Вы не имеете доступ к этому действию!",
                            cancellationToken: cancellationToken);
                        }
                        catch (Exception)
                        {
                            await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Неверный параметр id объекта!",
                            cancellationToken: cancellationToken);

                            return;
                        }
                    }
                    else // команда не найдена
                    {
                        await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Комманды не существует!",
                        cancellationToken: cancellationToken);
                    }
                }
            }
        }
    }
}
