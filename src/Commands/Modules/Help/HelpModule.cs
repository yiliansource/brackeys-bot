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

        [Command("help")]
        public async Task HelpAsync()
        {
            throw new NotImplementedException();
        }

        [Command("help")]
        public async Task HelpAsync(string command)
        {
            CommandInfo target = GetTargetCommand(command);
            await CommandHelpAsync(target, Context);
        }

        public static async Task CommandHelpAsync(CommandInfo command, ICommandContext context)
        {
            string title = command.Name;
            if (command.Aliases.Count > 0)
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

        private CommandInfo GetTargetCommand(string name)
            => Commands.Commands.FirstOrDefault(c => c.Aliases.Any(a => string.Equals(name, a, StringComparison.InvariantCultureIgnoreCase)));
        private ModuleInfo GetTargetModule(string name)
            => Commands.Modules.FirstOrDefault(m => string.Equals(name, ModuleService.SanitizeModuleName(m.Name), StringComparison.InvariantCultureIgnoreCase));

        private static EmbedFieldBuilder ParameterToEmbedField(ParameterInfo info)
            => new EmbedFieldBuilder()
                .WithName(info.Name)
                .WithValue(info.Summary.WithAlternative("No description provided."))
                .WithIsInline(true);
    }
}
