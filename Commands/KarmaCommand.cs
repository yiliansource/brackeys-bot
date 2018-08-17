using Discord;
using Discord.Commands;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    public class KarmaCommand : ModuleBase
    {
        private readonly CommandService commands;

        public KarmaCommand(CommandService commands)
        {
            this.commands = commands;
        }

        [Command("thanks")]
        [Alias("thank")]
        public async Task ThankUserCommand (SocketGuildUser user)
        {
            IUser source = Context.User, target = user;
            if (source == target)
            {
                await ReplyAsync("You can't thank yourself!");
                return;
            }

            KarmaTable table = BrackeysBot.Karma;

            int remainingMinutes;
            if (KarmaTable.CheckThanksCooldownExpired(source, target, out remainingMinutes))
            {
                table.ThankUser(source, target);

                int total = table.GetKarma(target);
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
        public async Task ModifyKarmaCommand (string operation, SocketGuildUser user, int amount)
        {
            StaffCommandHelper.EnsureStaff(Context.User as IGuildUser);

            KarmaTable table = BrackeysBot.Karma;

            switch(operation.ToLower())
            {
                case "set":
                    table.SetKarma(user, amount);
                    break;
                case "add":
                    table.AddKarma(user, amount);
                    break;
                case "remove":
                    table.RemoveKarma(user, amount);
                    break;

                default:
                    await ReplyAsync("Unknown karma operation.");
                    return;
            }

            await ReplyAsync($"{ user.Username } has { table.GetKarma(user) } points.");
        }
    }
}
