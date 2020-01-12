﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using BrackeysBot.Services;

namespace BrackeysBot.Commands
{
    [HideFromHelp]
    public sealed class HelpCommand : BrackeysBotModule
    {
        public CommandService Commands { get; set; }
        public ModuleService Modules { get; set; }
        public IServiceProvider Provider { get; set; }

        [Command("help")]
        [Summary("Displays a list of useable commands and modules.")]
        public async Task HelpAsync()
        {
            foreach (ModuleInfo module in Commands.Modules)
            {
                await DisplayModuleHelpAsync(module, Context);
            }
        }

        [Command("help")]
        [Summary("Displays more information about a module or command.")]
        public async Task HelpAsync(string identifier)
        {
            CommandInfo commandInfo = GetTargetCommand(identifier);
            ModuleInfo moduleInfo = GetTargetModule(identifier);

            if (commandInfo == null && moduleInfo == null)
            {
                await ReplyAsync($"A command or module with the name **{identifier}** could not be found.");
            }
            if (commandInfo != null && moduleInfo == null)
            {
                await DisplayCommandHelpAsync(commandInfo, Context);
            }
            if (commandInfo == null && moduleInfo != null)
            {
                await DisplayModuleHelpAsync(moduleInfo, Context);
            }
            if (commandInfo != null && moduleInfo != null)
            {
                StringBuilder reply = new StringBuilder()
                    .AppendLine("Both a module and a command were found, here are short summaries of both!")
                    .AppendLine()
                    .AppendLine($"**Command**: {commandInfo.Summary.WithAlternative("No description provided.")}")
                    .AppendLine($"**Module**: {moduleInfo.Summary.WithAlternative("No description provided.")}");

                await ReplyAsync(reply.ToString());
            }
        }

        public static async Task DisplayCommandHelpAsync(CommandInfo command, ICommandContext context)
        {
            string title = command.Name;
            if (command.Aliases.Count > 1)
                title += $" ({string.Join('|', command.Aliases)})";

            string prefix = ExtractPrefixFromContext(context);

            StringBuilder description = new StringBuilder()
                .AppendLine(command.Summary.WithAlternative("No description provided."))
                .AppendLine()
                .AppendLine("**Module**: " + command.Module.Name.Sanitize())
                .AppendLine("**Usage**: " + (prefix + command.Remarks).WithAlternative("No usage provided."));

            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description.ToString())
                .WithFields(command.Parameters.Select(InfoToEmbedField));

            ModuleColorAttribute moduleColor = command.Module.GetAttribute<ModuleColorAttribute>();
            if (moduleColor != null)
                builder.WithColor(moduleColor.Color);

            await builder.Build().SendToChannel(context.Channel);
        }
        public static async Task DisplayModuleHelpAsync(ModuleInfo module, ICommandContext context)
        {
            string prefix = ExtractPrefixFromContext(context);

            IEnumerable<CommandInfo> displayable = module.Commands
                .Where(c => c.CheckPreconditionsAsync(context).GetAwaiter().GetResult().IsSuccess
                    && !c.HasAttribute<HideFromHelpAttribute>());

            bool displayModuleHelp = displayable.Count() > 0 && !module.HasAttribute<HideFromHelpAttribute>();
            if (displayModuleHelp)
            {
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle(module.Name.Sanitize())
                    .WithFields(displayable.Select(c => InfoToEmbedField(c, prefix)));

                ModuleColorAttribute moduleColor = module.GetAttribute<ModuleColorAttribute>();
                if (moduleColor != null)
                    builder.WithColor(moduleColor.Color);

                if (!string.IsNullOrEmpty(module.Summary))
                    builder.WithDescription(module.Summary);

                await builder.Build()
                    .SendToChannel(context.Channel);
            }
        }

        private CommandInfo GetTargetCommand(string name)
            => Commands.Commands
                .FirstOrDefault(c => c.Aliases.Any(a => string.Equals(name, a, StringComparison.InvariantCultureIgnoreCase)));
        private ModuleInfo GetTargetModule(string name)
            => Commands.Modules
                .FirstOrDefault(m => string.Equals(name, m.Name.Sanitize(), StringComparison.InvariantCultureIgnoreCase));

        private static string ExtractPrefixFromContext(ICommandContext context)
            => (context as BrackeysBotContext)?.Configuration.Prefix ?? string.Empty;

        private static EmbedFieldBuilder InfoToEmbedField(ParameterInfo info)
            => new EmbedFieldBuilder()
                .WithName(info.Name)
                .WithValue(info.Summary.WithAlternative("No description provided."))
                .WithIsInline(true);
        private static EmbedFieldBuilder InfoToEmbedField(CommandInfo info, string prefix)
            => new EmbedFieldBuilder()
                .WithName(string.Concat(prefix, info.Remarks.WithAlternative(info.Name)))
                .WithValue(info.Summary.WithAlternative("No description provided."))
                .WithIsInline(false);
    }
}