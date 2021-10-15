﻿using System;
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
        private readonly CollabService _collabService;
        private readonly IServiceProvider _provider;
        private readonly LoggingService _log;

        // Characters which need escaping
        private static readonly string[] SensitiveCharacters = { "\\", "*", "_", "~", "`", "|", ">", "[", "(" };

        public CommandHandlerService(
            DiscordSocketClient discord,
            CommandService commands,
            CustomCommandService customCommands,
            DataService dataService,
            CollabService collabService,
            IServiceProvider provider,
            LoggingService log)
        {
            _discord = discord;
            _commands = commands;
            _customCommands = customCommands;
            _dataService = dataService;
            _collabService = collabService;
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
                            .WithDescription($"Command {SanitizeMarkdown(customCommandName)} does not exist!")
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
                else if (result.Error == CommandError.ObjectNotFound)
                {
                    await new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithDescription(result.ErrorReason)
                        .Build()
                        .SendToChannel(context.Channel);
                }
                else if (result.Error == CommandError.ParseFailed)
                {
                    await new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithDescription("Invalid command arguments.")
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
                    await _log.LogMessageAsync(new LogMessage(LogSeverity.Warning, GetType().Name.Prettify(), $"Unknown statement reached: Command={(command.IsSpecified ? command.Value : null)};Result={result}"));

                    EmbedBuilder reply = new EmbedBuilder()
                        .WithColor(Color.Red);

                    if (_dataService.Configuration.DeveloperRoleID != 0)
                    {
                        reply.WithDescription("Unknown error! I'll tell the developers ... :eyes:");
                        await context.Channel.SendMessageAsync(MentionUtils.MentionRole(_dataService.Configuration.DeveloperRoleID), false, reply.Build());
                    }
                    else
                    {
                        await reply.WithDescription("Unknown error! I wanted to tell the developers, but I don't know who they are ... :frowning:")
                            .Build()
                            .SendToChannel(context.Channel);
                    }
                }
            }
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg) || msg.Author.IsBot) return;

            int argPos = 0;
            if (!(s.Channel is IGuildChannel))
            {
                if (_collabService.IsActiveUser(msg.Author) &&
                    !msg.HasStringPrefix(_dataService.Configuration.Prefix, ref argPos))
                {
                    await _collabService.Converse(msg);
                }
                else
                {
                    await s.Channel.SendMessageAsync("I'm not available in DMs. Please use the Brackeys Discord Server to communicate with me!");
                }
                return;
            }           
            else if (!msg.HasStringPrefix(_dataService.Configuration.Prefix, ref argPos))
                return;

            var context = new BrackeysBotContext(msg, _provider);
            await _commands.ExecuteAsync(context, argPos, _provider);
        }

        private static string SanitizeMarkdown(string text)
        {
            foreach (string unsafeChar in SensitiveCharacters)
                text = text.Replace(unsafeChar, $"\\{unsafeChar}");
            return text;
        }
    }
}