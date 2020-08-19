using System.Text;
using System.Linq;
using System.Threading.Tasks;

using Discord.Commands;

using BrackeysBot.Core.Models;
using System.Globalization;

namespace BrackeysBot.Commands
{
    public partial class CustomCommandsModule : BrackeysBotModule
    {
        [Command("customcommands"), Alias("cclist", "ccl")]
        [Summary("Displays a list of custom commands.")]
        public async Task DisplayCommandsAsync()
        {
            StringBuilder builder = new StringBuilder()
                .AppendLine("Here is a list of useable commands!");

            var categorizedCommands = CustomCommands.GetCommands()
                .OrderBy(c => c.Name)
                .GroupBy(c => (c.Features.FirstOrDefault(f => f is CategoryFeature) as CategoryFeature)?.Category ?? null);

            foreach (var category in categorizedCommands)
            {
                string categoryDisplayName = new CultureInfo("en-US", false).TextInfo.ToTitleCase(category.Key ?? "uncategorized");

                builder.AppendLine();
                builder.AppendLine($"**{categoryDisplayName}**");
                foreach (var command in category)
                {
                    builder.AppendLine(command.Name);
                }
            }

            await GetDefaultBuilder()
                .WithDescription(builder.ToString())
                .Build()
                .SendToChannel(Context.Channel);
        }
    }
}
