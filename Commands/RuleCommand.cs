using Discord;
using Discord.Commands;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

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
                ruleTable.AddRule(id, contents);
                await ReplyAsync("Rule created.");
            }
        }

        [Command("removerule")]
        public async Task RemoveRule (int id)
        {
            StaffCommandHelper.EnsureStaff(Context.User as IGuildUser);

            RuleTable ruleTable = BrackeysBot.Rules;
            if (ruleTable.HasRule(id))
            {
                ruleTable.DeleteRule(id);
                await ReplyAsync("Rule deleted.");
            }
            else
            {
                await ReplyAsync("Rule doesn't exist.");
            }
        }

        [Command("allrules")]
        public async Task PrintAllRules ()
        {
            StaffCommandHelper.EnsureStaff(Context.User as IGuildUser);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("**Welcome to the official Brackeys discord server!**");
            builder.AppendLine();
            builder.AppendLine("Please take a moment to read the rules.");

            Dictionary<int, string> rules = BrackeysBot.Rules.Rules;
            foreach (int id in rules.Keys.OrderBy(k => k))
            {
                builder.AppendLine();
                builder.AppendLine($"**Rule { id }**");
                builder.AppendLine(rules[id]);
            }

            await ReplyAsync(builder.ToString());
        }
    }
}
