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

        private const string GIVEAWAY_EMOTE = "⭐";
        private const string GIVEAWAY_MESSAGE_IDENTIFIER = "giveaway_messageid";

        private static List<ulong> _blacklistedUsers = new List<ulong>();

        public GiveawayCommand (SettingsTable settings)
        {
            _settings = settings;
        }

        [Command("initgiveaway")]
        [HelpData("initgiveaway <message>", "Initializes a new giveaway, with a specified message.", AllowedRoles = UserType.Staff)]
        public async Task InitializeGiveaway ([Remainder] string message)
        {
            // Delete the invokation message
            await Context.Message.DeleteAsync();

            // Reply with the specified message
            var botMessage = await ReplyAsync(message);

            // Find the giveaway emote and add it as a reaction
            await botMessage.AddReactionAsync(new Emoji(GIVEAWAY_EMOTE));

            // Save the giveaway message id
            if (!_settings.Has(GIVEAWAY_MESSAGE_IDENTIFIER))
                _settings.Add(GIVEAWAY_MESSAGE_IDENTIFIER, botMessage.Id.ToString());
            else
                _settings.Set(GIVEAWAY_MESSAGE_IDENTIFIER, botMessage.Id.ToString());
        }

        [Command("performgiveaway")]
        [HelpData("performgiveaway <usercount> <includestaff>", "Performs a giveaway with a set number of winners.", AllowedRoles = UserType.Staff)]
        public async Task PerformGiveaway (int userCount, bool includeStaff = true)
        {
            // Delete the invokation message
            await Context.Message.DeleteAsync();

            // Verify that the giveaway has been initialized
            if (!_settings.Has(GIVEAWAY_MESSAGE_IDENTIFIER))
                throw new Exception("No giveaway message has been initialized.");

            // Check if the id is valid
            if (!ulong.TryParse(_settings.Get(GIVEAWAY_MESSAGE_IDENTIFIER), out ulong messageId))
                throw new Exception("Invalid giveaway message id.");

            // Get the randomized users
            var randomizedUsers = await GetRandomizedGiveawayUsers(messageId, includeStaff);

            if (randomizedUsers.Count() < 1)
            {
                await ReplyAsync("Apparently, no valid user has entered the giveaway yet ...");
                return;
            }

            // Select the users, capped by usercount
            IUser[] selectedUsers = randomizedUsers.Count() > userCount ? randomizedUsers.Take(userCount).ToArray() : randomizedUsers.ToArray();

            // Build the message
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("The drawn users of the giveaways are ...");

            foreach (IUser user in selectedUsers)
            {
                sb.AppendLine($"• { user.Mention }");
                _blacklistedUsers.Add(user.Id);
            }

            sb.AppendLine();
            sb.AppendLine("Congratulations to the winners!");

            await ReplyAsync(sb.ToString());
        }

        [Command("draw")]
        [HelpData("draw", "Draws a single user out of the giveaway pool. Also blacklists the user so he can't be drawn again.", AllowedRoles = UserType.Staff)]
        public async Task Draw (bool includeStaff = true)
        {
            // Verify that the giveaway has been initialized
            if (!_settings.Has(GIVEAWAY_MESSAGE_IDENTIFIER))
                throw new Exception("No giveaway message has been initialized.");

            // Check if the id is valid
            if (!ulong.TryParse(_settings.Get(GIVEAWAY_MESSAGE_IDENTIFIER), out ulong messageId))
                throw new Exception("Invalid giveaway message id.");
            
            // Get the randomized users
            var randomizedUsers = await GetRandomizedGiveawayUsers(messageId, includeStaff);

            if (randomizedUsers.Count() < 1)
            {
                await ReplyAsync("Apparently, no valid user has entered the giveaway yet ...");
                return;
            }

            // Get the first one (random)
            var drawnUser = randomizedUsers.First();
            _blacklistedUsers.Add(drawnUser.Id);

            string message = $"{ drawnUser.Mention } was drawn!";
            await ReplyAsync(message);
        }

        [Command("cleardraw")]
        [HelpData("cleardraw", "Clears the draw blacklist.", AllowedRoles = UserType.Staff)]
        public async Task ClearDraw()
        {
            int count = _blacklistedUsers.Count;

            _blacklistedUsers.Clear();

            string message = $"{ count } users have been cleared from the blacklist.";
            await ReplyAsync(message);
        }

        private async Task<IEnumerable<IGuildUser>> GetRandomizedGiveawayUsers(ulong messageId, bool includeStaff)
        {
            // Get the message by its id
            var giveawayMessage = await Context.Channel.GetMessageAsync(messageId) as IUserMessage;

            // Get all users that reacted to the message with the giveaway emote
            var giveawayUsers = await giveawayMessage.GetReactionUsersAsync(GIVEAWAY_EMOTE);
            // Project the users into IGuildUsers
            var guildUsers = await Context.Guild.GetUsersAsync(CacheMode.AllowDownload);
            var projectedGuildUsers = giveawayUsers.Select(u => guildUsers.First(g => g.Id == u.Id));

            // Filter out bots, blacklisted users and conditionally staff
            var users = projectedGuildUsers.Where(u => !u.IsBot && !_blacklistedUsers.Contains(u.Id));
            if (!includeStaff) users = users.Where(u => !(u as IGuildUser)?.HasStaffRole() ?? false);

            // Randomize the users
            Random rng = new Random();
            var randomizedUsers = users.OrderBy(u => rng.Next());

            return randomizedUsers;
        }
    }
}
