using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Data;

namespace TelegramBot
{
    public static class TelegramBotApplicationModule
    {
        private static string BotFlag = "part1"; // Первая или вторая часть задания сейчас активна?
        /* SERVICES PROVIDER DI CONTAINER */
        public static IServiceProvider _serviceProvider { get; set; }
        public static IServiceScope _serviceScope { get; set; }
        public static IApplicationServices? _applicationServices { get; set; }

        public static void Main(string[] args)
        {
            BotFlag = args.Length != 0 ? args[0] : "part1";
            /* CONFIGURATION */

            _serviceProvider = new ServiceCollection()
                .AddDbContext<TelegramBotDataBaseConnection>(act =>
                {
                    act.UseMySql("server=localhost;uid=DezareD;pwd=N1vs1nq12;database=telegramBot_test;Allow User Variables=true",
                        ServerVersion.AutoDetect("server=localhost;uid=DezareD;pwd=N1vs1nq12;database=telegramBot_test;Allow User Variables=true"));
                })
                .AddTransient<IApplicationServices, ApplicationServices>()
                .BuildServiceProvider(new ServiceProviderOptions()
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });

            _serviceScope = _serviceProvider.CreateScope();

            _applicationServices = (IApplicationServices)_serviceScope.ServiceProvider.GetRequiredService(typeof(IApplicationServices));

            TelegramBotClient client = new TelegramBotClient("5128846499:AAEfY3se9PbezH9YWmOL1lco2e0O4SqkBL4");

            /* START APPLICATION RECIVER */

            Task taskA = new Task(async () => await ApplicationReciverHub(client));
            taskA.Start();

            Console.ReadLine();
        }

        public static async Task ApplicationReciverHub(TelegramBotClient bot)
        {
            var me = await bot.GetMeAsync();
            Console.Title = me.Username;

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };

            Console.WriteLine($"Start listening for @{me.Username}");

            if (BotFlag == "part1")
            {

                var part1 = new Part1(_applicationServices);

                bot.StartReceiving(
                    part1.HandleUpdateAsync,
                    HandleErrorAsync,
                    receiverOptions,
                    cancellationToken: cts.Token);
            }
            else
            {
                var part2 = new Part2(_applicationServices);

                bot.StartReceiving(
                    part2.HandleUpdateAsync,
                    HandleErrorAsync,
                    receiverOptions,
                    cancellationToken: cts.Token);
            }

        }

        

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n\t[{apiRequestException.ErrorCode}]\n\t{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}