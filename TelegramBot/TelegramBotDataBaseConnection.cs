using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Serilog;
using System.Configuration;
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
            var builder = new DbContextOptionsBuilder<TelegramBotDataBaseConnection>().UseMySql(ConfigurationManager.AppSettings.Get("mysqlConnector"),
                        ServerVersion.AutoDetect(ConfigurationManager.AppSettings.Get("mysqlConnector")));


            return new TelegramBotDataBaseConnection(builder.Options);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                optionsBuilder.UseMySql(ConfigurationManager.AppSettings.Get("mysqlConnector"),
                        ServerVersion.AutoDetect(ConfigurationManager.AppSettings.Get("mysqlConnector")));
            }
            catch
            {
                Log.Logger.Error("Ошибка подключения к базе данных.");
            }
        }

    }
}
