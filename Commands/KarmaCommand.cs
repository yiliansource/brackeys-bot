using Discord.WebSocket;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using BrackeysBot.Data;

namespace BrackeysBot.Commands
{
    public class KarmaCommand : ModuleBase
    {
        private readonly KarmaTable _karmaTable;
        private readonly SettingsTable _settings;

        public KarmaCommand(KarmaTable karmaTable, SettingsTable settings)
        {
            _karmaTable = karmaTable;
            _settings = settings;
        }
        
        [Command("thanks"), Alias("thank")]
        public async Task ThankUserCommand()
        {
            EmbedBuilder eb = new EmbedBuilder()
                    .WithColor(new Color(0, 255, 255))
                    .WithTitle($"How to thank people:")
                    .WithDescription("Example: []thanks @Brackeys");

            await ReplyAsync(string.Empty, false, eb);
        }
        [Command("thanks"), Alias("thank")]
        [HelpData("thanks <user>", "Thank a user.")]
        public async Task ThankUserCommand ([Remainder]IGuildUser user)
        {
            IUser source = Context.User, target = user;
            if (source == target)
            {
                await ReplyAsync("You can't thank yourself!");
                return;
            }

            if (_karmaTable.CheckThanksCooldownExpired(source, target, out int remainingMinutes))
            {
                int.TryParse(_settings["thanks"], out int cooldown);
                _karmaTable.ThankUser(source, target, cooldown);

                int total = _karmaTable.GetKarma(target);
                string pointsDisplay = $"{ total } point{ (total != 1 ? "s" : "") }";
                await ReplyAsync($"{ user.Mention } has { pointsDisplay }.");
            }
            else
            {
                int hours = remainingMinutes / 60;
                int minutes = remainingMinutes % 60;

                string displayhours = $"{ hours } hour{ (minutes != 1 ? "s" : "") }";
                string displayminutes = $"{ minutes } minute{ (minutes != 1 ? "s" : "") }";

                await ReplyAsync($"{ source.Mention }, you can't thank that user yet, please wait { displayhours }{ (minutes > 0 ? $" and { displayminutes }" : "") }.");
            }
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
                    await ReplyAsync("Unknown karma operation.");
                    return;
            }

            await ReplyAsync($"{ UserHelper.GetDisplayName(user) } has { _karmaTable.GetKarma(user) } points.");
        }
    }
}
