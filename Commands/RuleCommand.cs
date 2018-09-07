using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

using BrackeysBot.Data;

namespace BrackeysBot.Commands
{
    public class RuleCommand : ModuleBase
    {
        private readonly RuleTable _ruleTable;
        private readonly SettingsTable _settings;

        public RuleCommand(RuleTable ruleTable, SettingsTable settings)
        {
            _ruleTable = ruleTable;
            _settings = settings;
        }

        [Command("rule")]
        [HelpData("rule <id>", "Quotes a rule.")]
        public async Task PrintRule (int id)
        {
            if (_ruleTable.Has(id))
            {
                EmbedBuilder eb = new EmbedBuilder()
                    .WithColor(new Color(0, 255, 255))
                    .WithTitle($"Rule { id }")
                    .WithDescription(_ruleTable.Get(id))
                    .WithFooter("To see all the rules go to #info.");

                await ReplyAsync(string.Empty, false, eb.Build());
            }
            else
            {
                await ReplyAsync("Invalid rule ID.");
            }
        }

        [Command("addrule")]
        [HelpData("addrule <id> <content>", "Creates a rule.", AllowedRoles = UserType.Staff)]
        public async Task AddRule (int id, [Remainder]string contents)
        {
            if (_ruleTable.Has(id))
            {
                await ReplyAsync("Rule already exists.");
            }
            else
            {
                _ruleTable.Add(id, contents);
                await ReplyAsync("Rule created.");
            }

            await UpdateOriginRuleMessage(Context.Guild);
        }

        [Command("setrule")]
        [HelpData("setrule <id> <content>", "Updates a rule.", AllowedRoles = UserType.Staff)]
        public async Task SetRule (int id, [Remainder]string contents)
        {
            if (_ruleTable.Has(id))
            {
                _ruleTable.Set(id, contents);
                await ReplyAsync("Rule updated.");
            }
            else
            {
                await ReplyAsync("Invalid rule ID.");
            }

            await UpdateOriginRuleMessage(Context.Guild);
        }

        [Command("removerule")]
        [HelpData("removerule <id>", "Removes a rule.", AllowedRoles = UserType.Staff)]
        public async Task RemoveRule (int id)
        {
            if (_ruleTable.Has(id))
            {
                _ruleTable.Remove(id);
                await ReplyAsync("Rule deleted.");
            }
            else
            {
                await ReplyAsync("Rule doesn't exist.");
            }

            await UpdateOriginRuleMessage(Context.Guild);
        }

        [Command("allrules")]
        [HelpData("allrules", "Prints all the rules.", AllowedRoles = UserType.Staff)]
        public async Task PrintAllRules ()
        {
            await ReplyAsync(BuildRuleMessage());
        }

        /// <summary>
        /// Builds the rule message.
        /// </summary>
        private string BuildRuleMessage ()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("**Welcome to the official Brackeys discord server!**");
            builder.AppendLine();
            builder.AppendLine("Please take a moment to read the rules.");

            Dictionary<int, string> rules = _ruleTable.Rules;
            foreach (int id in rules.Keys.OrderBy(k => k))
            {
                builder.AppendLine();
                builder.AppendLine($"**Rule { id }**");
                builder.AppendLine(rules[id]);
            }

            return builder.ToString();
        }
        /// <summary>
        /// Gets the origin rule message.
        /// </summary>
        private async Task<IUserMessage> GetOriginMessage(IGuild guild)
        {
            ulong id = ulong.Parse(_settings["rulemessage-id"]);
            var channels = await guild.GetChannelsAsync();
            var infoChannel = channels.First(c => c.Name.ToLower() == "info");
            var message = await (infoChannel as IMessageChannel).GetMessageAsync(id);

            return message as IUserMessage;
        }

        /// <summary>
        /// Updates the origin rule message with the rules from the rule table.
        /// </summary>
        private async Task UpdateOriginRuleMessage (IGuild guild)
        {
            var message = await GetOriginMessage(guild);
            string rules = BuildRuleMessage();

            await message.ModifyAsync(m => m.Content = rules);
        }
    }
}
