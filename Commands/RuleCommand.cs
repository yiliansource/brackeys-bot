using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

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

        [Command("addrule")]
        public async Task AddRule (int id, [Remainder]string contents)
        {
            StaffCommandHelper.EnsureStaff(Context.User as IGuildUser);

            RuleTable ruleTable = BrackeysBot.Rules;
            if (ruleTable.HasRule(id))
            {
                await ReplyAsync("Rule already exists.");
            }
            else
            {
                ruleTable.AddRule(id, contents);
                await ReplyAsync("Rule created.");
            }

            await UpdateOriginRuleMessage(Context.Guild);
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
                await ReplyAsync("Invalid rule ID.");
            }

            await UpdateOriginRuleMessage(Context.Guild);
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

            await UpdateOriginRuleMessage(Context.Guild);
        }

        [Command("allrules")]
        public async Task PrintAllRules ()
        {
            StaffCommandHelper.EnsureStaff(Context.User as IGuildUser);

            string rules = GetRulesMessage();
            await ReplyAsync(rules);
        }

        private static string GetRulesMessage ()
        {
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

            return builder.ToString();
        }

        private static async Task UpdateOriginRuleMessage (IGuild guild)
        {
            var message = await GetOriginMessage(guild);
            await UpdateOriginRuleMessage(message);
        }
        private static async Task<IUserMessage> GetOriginMessage (IGuild guild)
        {
            ulong id = ulong.Parse(BrackeysBot.Settings["rulemessage-id"]);
            var channels = await guild.GetChannelsAsync();
            var infoChannel = channels.First(c => c.Name.ToLower() == "info");
            var message = await (infoChannel as IMessageChannel).GetMessageAsync(id);

            return message as IUserMessage;
        }
        private static async Task UpdateOriginRuleMessage(IUserMessage originMessage)
        {
            // Get the origin message ID
            string rules = GetRulesMessage();

            await originMessage.ModifyAsync(m => m.Content = rules);
        }
    }
}
