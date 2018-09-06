using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using System.Text;

namespace BrackeysBot.Commands
{
    public class GiveawayCommand : ModuleBase
    {
        private readonly SettingsTable _settings;

        private const string GIVEAWAY_EMOTE = "brackeys";
        private const string GIVEAWAY_MESSAGE_IDENTIFIER = "giveaway_messageid";

        public GiveawayCommand (SettingsTable settings)
        {
            _settings = settings;
        }

        [Command("initgiveaway")]
        [HelpData("initgiveaway <message>", "Initializes a new giveaway, with a specified message.", HelpMode = "mod")]
        public async Task InitializeGiveaway ([Remainder] string message)
        {
            (Context.User as IGuildUser).EnsureStaff();

            // Delete the invokation message
            await Context.Message.DeleteAsync();

            // Reply with the specified message
            var botMessage = await ReplyAsync(message);

            // Find the giveaway emote and add it as a reaction
            var giveawayEmote = Context.Guild.Emotes.First(e => e.Name == GIVEAWAY_EMOTE);
            await botMessage.AddReactionAsync(giveawayEmote);

            // Save the giveaway message id
            if (!_settings.Has(GIVEAWAY_MESSAGE_IDENTIFIER))
                _settings.Add(GIVEAWAY_MESSAGE_IDENTIFIER, botMessage.Id.ToString());
            else
                _settings.Set(GIVEAWAY_MESSAGE_IDENTIFIER, botMessage.Id.ToString());
        }

        [Command("performgiveaway")]
        [HelpData("performgiveaway <usercount> <includestaff>", "Performs a giveaway with a set number of winners.", HelpMode = "mod")]
        public async Task PerformGiveaway (int userCount, bool includeStaff = true)
        {
            (Context.User as IGuildUser).EnsureStaff();

            if (!_settings.Has(GIVEAWAY_MESSAGE_IDENTIFIER))
                throw new Exception("No giveaway message has been initialized.");

            if (!ulong.TryParse(_settings.Get(GIVEAWAY_MESSAGE_IDENTIFIER), out ulong messageId))
                throw new Exception("Invalid giveaway message id.");

            var giveawayMessage = await Context.Channel.GetMessageAsync(messageId) as IUserMessage;

            var giveawayEmote = Context.Guild.Emotes.First(e => e.Name == GIVEAWAY_EMOTE);
            var giveawayUsers = await giveawayMessage.GetReactionUsersAsync(giveawayEmote.Name);

            var users = giveawayUsers.Where(u => !u.IsBot);
            if (!includeStaff) users = giveawayUsers.Where(u => !(u as IGuildUser).HasStaffRole());
            
            Random rng = new Random();
            var randomizedUsers = users.OrderBy(u => rng.Next());

            IEnumerable<IUser> selectedUsers = randomizedUsers.Count() > userCount ? randomizedUsers.Take(userCount) : randomizedUsers;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("The drawn users of the giveaways are ...");

            foreach (IUser user in selectedUsers)
                sb.AppendLine($"• { user.Mention }");

            sb.AppendLine();
            sb.AppendLine("Congratulations to the winners!");

            await ReplyAsync(sb.ToString());
        }
    }
}
