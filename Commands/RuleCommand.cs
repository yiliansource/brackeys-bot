using Discord;
using Discord.Commands;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrackeysBot.Commands
{
    public class RuleCommand : ModuleBase
    {
        private readonly CommandService commands;

        public RuleCommand(CommandService commands)
        {
            this.commands = commands;
        }

        [Command("rule")]
        public async Task PrintRule (int id)
        {
            StaffCommandHelper.EnsureStaff(Context.User as IGuildUser);

            RuleTable ruleTable = BrackeysBot.Rules;
            if (ruleTable.HasRule(id))
            {
                EmbedBuilder eb = new EmbedBuilder()
                    .WithColor(new Color(0, 255, 255))
                    .WithTitle($"Rule { id }")
                    .WithDescription(ruleTable[id])
                    .WithFooter("To see all the rules go to #info.");

                await ReplyAsync(string.Empty, false, eb);
            }
            else
            {
                await ReplyAsync("Invalid rule ID.");
            }
        }

        [Command("setrule")]
        public async Task SetRule (int id, [Remainder]string contents)
        {
            StaffCommandHelper.EnsureStaff(Context.User as IGuildUser);

            RuleTable ruleTable = BrackeysBot.Rules;
            if (ruleTable.HasRule(id))
            {
                ruleTable[id] = contents;
                await ReplyAsync("Rule updated.");
            }
            else
            {
                ruleTable[id] = contents;
                await ReplyAsync("Rule created.");
            }
        }
    }
}
