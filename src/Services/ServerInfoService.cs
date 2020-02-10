using System;
using System.Timers;

using Discord;
using Discord.WebSocket;

namespace BrackeysBot.Services
{
    public class ServerInfoService : BrackeysBotService, IInitializeableService
    {
        private readonly DiscordSocketClient _discord;
        private readonly DataService _dataService;

        private Timer _timer;
        private BotConfiguration _config;

        public ServerInfoService(DiscordSocketClient discord ,DataService dataService)
        {
            _discord = discord;
            _dataService = dataService;
        }

        public void Initialize() 
        {
            _config = _dataService.Configuration;
            _timer = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds)
            {
                AutoReset = true,
                Enabled = true
            };

            _timer.Elapsed += (s, e) => UpdateCategoryCount();
            _timer.Start();
        }

        private void UpdateCategoryCount() 
        {
            if (ClientUpAndRunning() && CategoryConfigurationAvailable()) 
            {
                int memberCount = _discord.GetGuild(_config.GuildID).MemberCount;
                ICategoryChannel channel = _discord.GetChannel(_config.InfoCategoryId) as ICategoryChannel;
                string categoryName = _config.InfoCategoryDisplay.Replace("%s%", $"{memberCount}");

                channel.ModifyAsync(x => x.Name = categoryName);
            }
        }

        private bool ClientUpAndRunning() 
        {
            // This service is active before the client is initialized, we should check for client and guild to be available
            return _discord != null && _discord.GetGuild(_config.GuildID) != null;
        }

        private bool CategoryConfigurationAvailable() 
        {
            // Config will likely be set once the bot is running, this should prevent unexpected behaviour
            return _config.InfoCategoryDisplay != null && _config.InfoCategoryId > 0;
        }
    }
}