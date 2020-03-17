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
        private readonly LoggingService _log;

        public CommandHandlerService(
            DiscordSocketClient discord,
            CommandService commands,
            CustomCommandService customCommands,
            DataService dataService,
            IServiceProvider provider,
            LoggingService log)
        {
            _discord = discord;
            _commands = commands;
            _customCommands = customCommands;
            _dataService = dataService;
            _provider = provider;
            _log = log;
        }

        public void Initialize()
        {
            _discord.MessageReceived += HandleCommandAsync;
            _commands.CommandExecuted += OnCommandExecutedAsync;

            _commands.AddTypeReader<GuildUserProxy>(new GuildUserProxyTypeReader());
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
                    int prefixLength = _dataService.Configuration.Prefix.Length;
                    
                    string customCommandStr = context.Message.Content.Substring(prefixLength);
                    string[] splitCommand = customCommandStr.Split(' ');
                    string customCommandName = splitCommand[0];
                    string[] args = splitCommand.Length > 1 ? splitCommand[1..(splitCommand.Length - 1)] : new string[0];

                    if (_customCommands.TryGetCommand(customCommandName, args, out CustomCommand customCommand))
                    {
                        await customCommand.ExecuteCommand(context);
                    } 
                    else 
                    {
                        await new EmbedBuilder()
                            .WithColor(Color.Red)
                            .WithDescription($"Command {customCommandName} does not exist!")
                            .Build()
                            .SendToChannel(context.Channel);
                    }
                }
                else if (result.Error == CommandError.UnmetPrecondition)
                {
                    await new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithDescription("Access denied.")
                        .Build()
                        .SendToChannel(context.Channel);
                }
                else if (result is ExecuteResult executeResult)
                {
                    await new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithTitle(executeResult.Exception.GetType().Name.Prettify())
                        .WithDescription(executeResult.Exception.Message)
                        .Build()
                        .SendToChannel(context.Channel);
                }
                else 
                {
                    await new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithDescription("Unknown error! (I did create a log entry for the developers :))")
                        .Build()
                        .SendToChannel(context.Channel);
                    await _log.LogMessageAsync(new LogMessage(LogSeverity.Warning, GetType().Name.Prettify(), $"Unknown statement reached: Command={(command.IsSpecified ? command.Value : null)};Result={result}"));
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