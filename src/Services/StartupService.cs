using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BrackeysBot.Services
{
    public class StartupService : BrackeysBotService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _discord;
        private readonly DataService _data;
        private readonly LoggingService _log;

        public StartupService(IServiceProvider provider, DiscordSocketClient discord, DataService data, LoggingService log)
        {
            _provider = provider;
            _discord = discord;
            _data = data;
            _log = log;
        }

        public async Task StartAsync()
        {
            string discordToken = _data.Configuration.Token;
            if (string.IsNullOrEmpty(discordToken))
            {
                await _log.LogMessageAsync(new LogMessage(LogSeverity.Error, "Startup", "The login token in the config.yaml file was not set."));
                return;
            }

            await _discord.LoginAsync(TokenType.Bot, discordToken);
            await _discord.StartAsync();
        }
    }
}