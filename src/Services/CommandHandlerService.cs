using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using BrackeysBot.Commands;
using BrackeysBot.Core.Models;

namespace BrackeysBot.Services
{
    public class CommandHandlerService : BrackeysBotService, IInitializeableService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly CustomCommandService _customCommands;
        private readonly DataService _dataService;
        private readonly IServiceProvider _provider;

        public CommandHandlerService(
            DiscordSocketClient discord,
            CommandService commands,
            CustomCommandService customCommands,
            DataService dataService,
            IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _customCommands = customCommands;
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
                    await HelpModule.DisplayCommandHelpAsync(command.Value, context);
                }
                else if (result.Error == CommandError.UnknownCommand)
                {
                    string customCommandName = context.Message.Content.Substring(_dataService.Configuration.Prefix.Length);
                    if (_customCommands.TryGetCommand(customCommandName, out CustomCommand customCommand))
                    {
                        await customCommand.ExecuteCommand(context);
                    }
                }
                else
                {
                    if (result is ExecuteResult executeResult)
                    {
                        await new EmbedBuilder()
                            .WithColor(Color.Red)
                            .WithTitle(executeResult.Exception.GetType().Name.Prettify())
                            .WithDescription(executeResult.Exception.Message)
                            .Build()
                            .SendToChannel(context.Channel);
                    }
                }
            }
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return;

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