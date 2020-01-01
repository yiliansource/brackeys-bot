using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BrackeysBot.Services
{
    public class StartupService : BrackeysBotService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _discord;
        private readonly DataService _dataService;

        public StartupService(IServiceProvider provider, DiscordSocketClient discord, DataService dataService)
        {
            _provider = provider;
            _dataService = dataService;
            _discord = discord;
        }

        public async Task StartAsync()
        {
            string discordToken = _dataService.Configuration.Token;

            await _discord.LoginAsync(TokenType.Bot, discordToken);
            await _discord.StartAsync();
            await _discord.SetStatusAsync(UserStatus.AFK);
            await _discord.SetGameAsync("being a broken bot.");
        }
    }
}