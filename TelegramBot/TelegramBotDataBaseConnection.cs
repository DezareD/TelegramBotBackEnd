using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Serilog;
using TelegramBot.Data;

namespace TelegramBot
{
    public class TelegramBotDataBaseConnection : DbContext, IDesignTimeDbContextFactory<TelegramBotDataBaseConnection>
    {
        public TelegramBotDataBaseConnection(DbContextOptions<TelegramBotDataBaseConnection> options)
            : base(options)
        {
        }

        public TelegramBotDataBaseConnection() : base() { }

        public DbSet<Applicant> applicants { get; set; }
        public DbSet<Client> clients { get; set; }
        public DbSet<ArtObject> artObjects { get; set; }

        public TelegramBotDataBaseConnection CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<TelegramBotDataBaseConnection>().UseMySql("server=localhost;uid=DezareD;pwd=N1vs1nq12;database=telegramBot_test;Allow User Variables=true",
                        ServerVersion.AutoDetect("server=localhost;uid=DezareD;pwd=N1vs1nq12;database=telegramBot_test;Allow User Variables=true"));


            return new TelegramBotDataBaseConnection(builder.Options);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                optionsBuilder.UseMySql("server=localhost;uid=DezareD;pwd=N1vs1nq12;database=telegramBot_test;Allow User Variables=true",
                        ServerVersion.AutoDetect("server=localhost;uid=DezareD;pwd=N1vs1nq12;database=telegramBot_test;Allow User Variables=true"));
            }
            catch
            {
                Log.Logger.Error("Ошибка подключения к базе данных.");
            }
        }

    }
}
