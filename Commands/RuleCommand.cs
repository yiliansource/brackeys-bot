using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

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
                throw new System.Exception("Invalid rule ID.");
            }
        }

        [Command("setrule"), Alias("addrule")]
        [HelpData("setrule <id> <content>", "Updates a rule.", AllowedRoles = UserType.Staff)]
        public async Task SetRule (int id, [Remainder]string contents)
        {
            if (_ruleTable.Has(id))
            {
                _ruleTable.Set(id, contents);
                await ReplyAsync($"I updated rule { id } for you!");
            }
            else
            {
                _ruleTable.Add(id, contents);
                await ReplyAsync($"I created rule { id } for you!");
            }
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
                throw new System.Exception("Rule doesn't exist.");
            }
        }

        [Command("rules")]
        [HelpData("rules", "Prints all the rules.", AllowedRoles = UserType.StaffGuru)]
        public async Task PrintAllRules ()
        {
            StringBuilder builder = new StringBuilder();

            string[] rules = _ruleTable.Rules;
            for (int i = 0; i < rules.Length; i++)
            {
                builder.AppendLine($"{ i + 1 } - { rules[i] }");
            }

            await ReplyAsync(builder.ToString());
        }
    }
}
