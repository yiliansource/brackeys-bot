using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using BrackeysBot.Services;

namespace BrackeysBot
{
    public sealed class BrackeysBot
    {
        public static Task StartAsync()
            => new BrackeysBot().RunAsync();

        private DataService _dataService;

        public BrackeysBot()
        {
            _dataService = new DataService();

            Console.Title = nameof(BrackeysBot);
        }

        public async Task RunAsync()
        {
            var services = CreateServices();

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<LoggingService>();
            provider.GetRequiredService<CommandHandlerService>();

            await provider.GetRequiredService<ModuleService>().Initialize();
            await provider.GetRequiredService<StartupService>().StartAsync();

            await Task.Delay(-1);
        }

        private IServiceCollection CreateServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton(_dataService)
                .AddBrackeysBotServices();
        }
    }
}