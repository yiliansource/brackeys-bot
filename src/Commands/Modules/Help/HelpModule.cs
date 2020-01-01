using System;
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
    public sealed class HelpModule : BrackeysBotModule
    {
        public CommandService Commands { get; set; }
        public ModuleService Modules { get; set; }
        public IServiceProvider Provider { get; set; }

        [Command("help")]
        [Summary("Displays a list of useable commands and modules.")]
        public async Task HelpAsync()
        {

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

            string prefix = (context as BrackeysBotContext).Configuration.Prefix;

            StringBuilder description = new StringBuilder()
                .AppendLine(command.Summary.WithAlternative("No description provided."))
                .AppendLine()
                .AppendLine("**Module**: " + ModuleService.SanitizeModuleName(command.Module.Name))
                .AppendLine("**Usage**: " + (prefix + command.Remarks).WithAlternative("No usage provided."));

            await new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description.ToString())
                .WithFields(command.Parameters.Select(ParameterToEmbedField))
                .Build()
                .SendToChannel(context.Channel);
        }
        public static async Task DisplayModuleHelpAsync(ModuleInfo module, ICommandContext context)
        {

        }

        private CommandInfo GetTargetCommand(string name)
            => Commands.Commands
                .FirstOrDefault(c => c.Aliases.Any(a => string.Equals(name, a, StringComparison.InvariantCultureIgnoreCase)));
        private ModuleInfo GetTargetModule(string name)
            => Commands.Modules
                .FirstOrDefault(m => string.Equals(name, ModuleService.SanitizeModuleName(m.Name), StringComparison.InvariantCultureIgnoreCase));

        private static EmbedFieldBuilder ParameterToEmbedField(ParameterInfo info)
            => new EmbedFieldBuilder()
                .WithName(info.Name)
                .WithValue(info.Summary.WithAlternative("No description provided."))
                .WithIsInline(true);
    }
}
