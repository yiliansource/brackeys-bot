using System;
using System.Threading.Tasks;
using System.Timers;

using Discord;
using Discord.WebSocket;

namespace BrackeysBot.Services
{
    public class ServerInfoService : BrackeysBotService, IInitializeableService
    {
        private readonly DiscordSocketClient _discord;
        private readonly DataService _dataService;
        private readonly LoggingService _loggingService;

        private Timer _timer;
        private BotConfiguration _config;

        public ServerInfoService(DiscordSocketClient discord, DataService dataService, LoggingService loggingService)
        {
            _discord = discord;
            _dataService = dataService;
            _loggingService = loggingService;
        }

        public void Initialize() 
        {
            _config = _dataService.Configuration;
            _timer = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds)
            {
                AutoReset = true,
                Enabled = true
            };

            _timer.Elapsed += async (s, e) => await UpdateCategoryCount();
            _timer.Start();
        }

        private async Task UpdateCategoryCount() 
        {
            if (ClientUpAndRunning() && CategoryConfigurationAvailable()) 
            {
                int memberCount = _discord.GetGuild(_config.GuildID).MemberCount;
                Console.WriteLine($"MemberCount is {memberCount}");
                ICategoryChannel channel = _discord.GetChannel(_config.InfoCategoryId) as ICategoryChannel;
                Console.WriteLine($"Category is {channel.Id}:{channel.Name}:{channel.CreatedAt}");
                string categoryName = _config.InfoCategoryDisplay.Replace("%s%", $"{memberCount}");
                Console.WriteLine($"Result name will be {categoryName}");

                await channel.ModifyAsync(x => x.Name = categoryName);
                Console.WriteLine("Modified Channel");
            } 
            else 
                await _loggingService.LogMessageAsync(new LogMessage(LogSeverity.Verbose, "ServerInfoService", $"Discord is {_discord}, Guild is {_discord.GetGuild(_config.GuildID)}, InfoCategory is {_config.InfoCategoryDisplay}, InfoCategoryId is {_config.InfoCategoryId}"));
        }

        private bool ClientUpAndRunning() 
        {
            Console.WriteLine($"Discord is {_discord}, Guild is {_discord.GetGuild(_config.GuildID)}");
            // This service is active before the client is initialized, we should check for client and guild to be available
            return _discord != null && _discord.GetGuild(_config.GuildID) != null;
        }

        private bool CategoryConfigurationAvailable() 
        {
            Console.WriteLine($"InfoCategory is {_config.InfoCategoryDisplay}, InfoCategoryId is {_config.InfoCategoryId}");
            // Config will likely be set once the bot is running, this should prevent unexpected behaviour
            return _config.InfoCategoryDisplay != null && _config.InfoCategoryId > 0;
        }
    }
}