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
            if (table.CheckThanksCooldownExpired(source, target, out remainingMinutes))
            {
                table.ThankUser(source, target);

                int total = table.GetKarma(target);
                await ReplyAsync($"{ user.Mention } has { total } point{ (total != 1 ? "s" : "") }.");
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
    }
}
