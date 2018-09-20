using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    class KarmaCommand : ModuleBase
    {
        private readonly KarmaTable _karmaTable;
        private readonly SettingsTable _settings;

        public KarmaCommand(KarmaTable karmaTable, SettingsTable settings)
        {
            _karmaTable = karmaTable;
            _settings = settings;
        }
        
        [Command("thanks"), Alias("thank", "thank you")]
        public async Task ThankUserCommand()
        {
            EmbedBuilder eb = new EmbedBuilder()
                    .WithColor(new Color(0, 255, 255))
                    .WithTitle($"How to thank people:")
                    .WithDescription("Example: []thanks @Brackeys");

            var message = await ReplyAsync(string.Empty, false, eb.Build());
            _ = message.TimedDeletion(5000);
        }
        [Command("thanks"), Alias("thank", "thank you")]
        [HelpData("thanks <user>", "Thank a user.")]
        public async Task ThankUserCommand ([Remainder]IGuildUser user)
        {
            IUser source = Context.User, target = user;
            if (source == target)
                throw new System.Exception("You cannot thank yourself.");

            _karmaTable.AddKarma(target);

            int total = _karmaTable.GetKarma(target);
            string pointsDisplay = $"{ total } point{ (total != 1 ? "s" : "") }";
            var message = await ReplyAsync($"{ user.GetDisplayName() } has { pointsDisplay }.");
            _ = message.TimedDeletion(5000);
        }
        [Command("thanks"), Alias("thank", "thank you")]
        public async Task ThankUserCommand(params IGuildUser[] users)
        {
            IUser source = Context.User;

            System.Collections.Generic.List<string> thankedUserMessages = new System.Collections.Generic.List<string>();
            foreach (IGuildUser user in users)
            {
                if (source == user)
                    continue;

                _karmaTable.AddKarma(user);

                int total = _karmaTable.GetKarma(user);
                string pointsDisplay = $"{ total } point{ (total != 1 ? "s" : "") }";
                thankedUserMessages.Add($"{ user.GetDisplayName() } has { pointsDisplay }");
            }

            string content = string.Join(", ", thankedUserMessages.ToArray()) + ".";
            var message = await ReplyAsync(content);
            _ = message.TimedDeletion(10000);
        }

        [Command("karma")]
        [HelpData("karma (add / remove / set) <user> <value>", "Modifies a user's karma points.", AllowedRoles = UserType.Staff)]
        public async Task ModifyKarmaCommand (string operation, SocketGuildUser user, int amount)
        {
            (Context.User as IGuildUser).EnsureStaff();

            switch (operation.ToLower())
            {
                case "set":
                    _karmaTable.SetKarma(user, amount);
                    break;
                case "add":
                    _karmaTable.AddKarma(user, amount);
                    break;
                case "remove":
                    _karmaTable.RemoveKarma(user, amount);
                    break;

                default:
                    throw new System.Exception("Unknown karma operation.");
            }

            await ReplyAsync($"{ UserHelper.GetDisplayName(user) } has { _karmaTable.GetKarma(user) } points.");
        }
    }
}
