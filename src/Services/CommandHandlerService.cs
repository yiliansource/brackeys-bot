using System.Threading.Tasks;
using System;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using BrackeysBot.Commands;

namespace BrackeysBot.Services
{
    public class CommandHandlerService : BrackeysBotService, IInitializeableService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly DataService _dataService;
        private readonly IServiceProvider _provider;

        public CommandHandlerService(
            DiscordSocketClient discord,
            CommandService commands,
            DataService dataService,
            IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _dataService = dataService;
            _provider = provider;
        }

        public void Initialize()
        {
            _discord.MessageReceived += HandleCommandAsync;
            _commands.CommandExecuted += OnCommandExecutedAsync;
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                if (result.Error == CommandError.BadArgCount)
                {
                    await HelpCommand.DisplayCommandHelpAsync(command.Value, context);
                }
                else
                {
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                }
            }
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;

            int argPos = 0;
            if (!(msg.HasStringPrefix(_dataService.Configuration.Prefix, ref argPos) ||
                msg.HasMentionPrefix(_discord.CurrentUser, ref argPos)) ||
                msg.Author.IsBot)
                return;

            var context = new BrackeysBotContext(msg, _provider);
            await _commands.ExecuteAsync(context, argPos, _provider);
        }
    }
}