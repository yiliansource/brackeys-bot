using System;
using Microsoft.EntityFrameworkCore;

namespace BrackeysBot.Models.Database
{
    public class DatabaseContext : DbContext
    {
        private readonly BotConfiguration _config;

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Infraction> Infractions { get; set; }
        public DbSet<TemporaryInfraction> TemporaryInfractions { get; set; }
        public DbSet<UserData> UserData { get; set; }
        public DbSet<LoggedMessage> Messages { get; set; }
        public DbSet<InfractionsMessages> InfrMsgs { get; set; }

        public DatabaseContext(BotConfiguration config)
        {
            Console.WriteLine("Creating");
            _config = config;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Console.WriteLine("Configuring");
            Console.WriteLine(_config.DatabaseSchema);
            optionsBuilder.UseMySql(
                $"server={_config.DatabaseHost};database={_config.DatabaseSchema};user={_config.DatabaseUser};password={_config.DatabasePass}");
        }
    }
}