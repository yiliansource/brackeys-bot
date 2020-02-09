using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using BrackeysBot.Services;

namespace BrackeysBot
{
    public sealed class BrackeysBot
    {
        public static Task StartAsync()
            => new BrackeysBot().RunAsync();

        public BrackeysBot()
        {
            Console.Title = $"{nameof(BrackeysBot)} v{Version.ShortVersion} (Discord.Net v{Version.DiscordVersion})";
        }

        public async Task RunAsync()
        {
            var services = CreateServices();

            var provider = services.BuildServiceProvider();
            provider.InitializeServices();

            await provider.GetRequiredService<StartupService>().StartAsync();

            await Task.Delay(-1);
        }

        private IServiceCollection CreateServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 100
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    DefaultRunMode = RunMode.Async
                }))
                .AddBrackeysBotServices();
        }
    }
}