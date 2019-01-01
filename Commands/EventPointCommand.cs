using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    public class EventPointCommand : ModuleBase
    {
        private readonly SettingsTable _settings;
        private readonly EventPointTable _pointTable;
        private readonly LeaderboardNavigator _navigator;

        private const string LEFT_ARROW = "⬅";
        private const string RIGHT_ARROW = "➡";

        public EventPointCommand(SettingsTable settings, EventPointTable pointTable, LeaderboardNavigator navigator)
        {
            _settings = settings;
            _pointTable = pointTable;
            _navigator = navigator;
        }

        [Command("eventpoints"), Alias("points", "ep")]
        [HelpData("points", "Displays your current event points.")]
        public async Task DisplayEventPoints()
        {
            var user = Context.User;
            int points = _pointTable.GetPoints(user);

            string pointsDisplay = $"{ points } point{ (points != 1 ? "s" : "") }";
            await ReplyAsync($"{ (user as IGuildUser).GetDisplayName() }, you have { pointsDisplay }.");
        }

        [Command("eventpoints"), Alias("points", "ep")]
        [HelpData("points <user>", "Displays the points of a specific user.")]
        public async Task DisplayEventPoints(SocketGuildUser user)
        {
            int points = _pointTable.GetPoints(user);

            string pointsDisplay = $"{ points } point{ (points != 1 ? "s" : "") }";
            await ReplyAsync($"{ (user as IGuildUser).GetDisplayName() } has { pointsDisplay }.");
        }
        
        [Command("eventpoints"), Alias("points", "ep")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("eventpoints (add / remove / set) <user> <value>", "Modifies a user's event points.")]
        public async Task ModifyKarmaCommand(string operation, SocketGuildUser user, int amount)
        {
            switch (operation.ToLower())
            {
                case "set":
                    _pointTable.SetPoints(user, amount);
                    break;
                case "add":
                    _pointTable.AddPoints(user, amount);
                    break;
                case "remove":
                    _pointTable.RemovePoints(user, amount);
                    break;

                default:
                    throw new Exception("Unknown karma operation.");
            }

            EmbedBuilder builder = new EmbedBuilder()
                .WithColor(new Color(162, 219, 160))
                .WithTitle($"Points updated!")
                .WithDescription($"{ UserHelper.GetDisplayName(user) } has { _pointTable.GetPoints(user) } event points.");

            await ReplyAsync(string.Empty, false, builder);

            // If the settings tell you to automatically update the roles, do it.
            if (_settings.Has("top-autoupdate"))
            {
                if (bool.Parse(_settings.Get("top-autoupdate")))
                {
                    await UpdateTopUsersWithRoles(false);
                }
            }
        }

        [Command("updateeventroles"), Alias("uer")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("updateeventroles [with-reply]", "Updates the top users of the leaderboard with the event top role, and conditionally includes a reply with the updated users.")]
        public async Task UpdateTopUsersWithRoles(bool withReply = true)
        {
            // Fetch the setting variables
            string topRoleName = _settings["top-role-name"];
            int topRoleMaxCount = int.Parse(_settings["top-role-maxcount"]);

            // Fetch the top N places
            var top = _pointTable.GetLeaderboardPlaces(0, topRoleMaxCount);

            // Get all users in the guild
            var allUsers = await Context.Guild.GetUsersAsync();

            // Get all users that are included in the NEW top N, and all users that were in the PREVIOUS top N
            var topUsers = allUsers.Where(u => top.Any(k => k.Key == u.Id));
            var prevTopUsers = allUsers.Where(u => u.HasRole(topRoleName));

            // The ex top N are the users that are no longer in the top N, and vice-versa
            var exTop = prevTopUsers.Except(topUsers);
            var newTop = topUsers.Except(prevTopUsers);

            // Fetch the top role, by its name
            var topRole = Context.Guild.Roles.FirstOrDefault(r => string.Equals(r.Name, topRoleName, StringComparison.CurrentCultureIgnoreCase));
            // And apply the new role distribution
            foreach (IGuildUser user in exTop)
                await user.RemoveRoleAsync(topRole);
            foreach (IGuildUser user in newTop)
                await user.AddRoleAsync(topRole);

            if (withReply)
            {
                string newTopX = string.Join(", ", topUsers.Select(u => u.GetDisplayName()).ToArray());

                EmbedBuilder reply = new EmbedBuilder()
                    .WithColor(new Color(162, 219, 160))
                    .WithTitle("Event Roles Updated!")
                    .WithDescription($"**New Top { topRoleMaxCount } Users**\n{ newTopX }");

                await ReplyAsync(string.Empty, false, reply);
            }
        }
        
        [Command("leaderboard"), Alias("top")]
        [HelpData("leaderboard [starting-rank]", "Shows the leaderboard, either from rank 1, or from a specified rank.")]
        public async Task ShowLeaderboard(int startingRank = 1)
        {
            if (_pointTable.RegisteredUsers < 1)
            {
                var eb = new EmbedBuilder()
                    .WithColor(new Color(162, 219, 160))
                    .WithTitle("This place is empty.")
                    .WithDescription("Seems like no one is on the leaderboard just yet.");

                await ReplyAsync(string.Empty, false, eb);
                return;
            }

            int startingRankIndex = Math.Clamp(startingRank - 1, 0, _pointTable.RegisteredUsers - 1);

            int.TryParse(_settings["leaderboard-pagesize"], out int pagesize);

            var builder = await BuildForNextPlaces(_pointTable, Context.Guild, startingRankIndex, pagesize);
            var message = await ReplyAsync(string.Empty, false, builder);

            _ = message.AddReactionAsync(new Emoji(LEFT_ARROW));
            _ = message.AddReactionAsync(new Emoji(RIGHT_ARROW));

            _navigator.AddTrackedMessage(new LeaderboardNavigator.LeaderboardDisplayData { DisplayIndex = startingRankIndex, Message = message, Guild = Context.Guild });
        }
        
        /// <summary>
        /// Returns an <see cref="Embed"/> that contains the message for the leaderboard display.
        /// </summary>
        private static async Task<Embed> BuildForNextPlaces(EventPointTable table, IGuild guild, int startIndex, int pagesize)
        {
            IEnumerable<KeyValuePair<ulong, int>> places = table.GetLeaderboardPlaces(startIndex, pagesize);

            EmbedBuilder eb = new EmbedBuilder()
                .WithColor(new Color(162, 219, 160))
                .WithTitle("Event Point Leaderboard:");

            int count = places.Count();
            for (int i = 0; i < count; i++)
            {
                KeyValuePair<ulong, int> place = places.ElementAt(i);

                IGuildUser user = await guild.GetUserAsync(place.Key);
                string username = user?.GetDisplayName() ?? "<invalid-user>"; // if the guild doesn't contain the user, return "<invalid-user>"

                string pointsDisplay = $"{ place.Value } point{ (place.Value != 1 ? "s" : "") }";

                string title = GetPlaceStringRepresentation(startIndex + i);
                string content = $"{ username } with { pointsDisplay }.";

                eb.AddField(title, content);
            }

            return eb;
        }

        /// <summary>
        /// Gets the string representation of a place, given its index. (0 -> "1st Place", 1 -> "2nd Place")
        /// </summary>
        private static string GetPlaceStringRepresentation(int placeIndex)
        {
            int place = placeIndex + 1;
            switch (place % 100)
            {
                case 11:
                case 12:
                case 13:
                    return place + "th Place:";
            }

            switch (place % 10)
            {
                case 1:
                    return place + "st Place:";
                case 2:
                    return place + "nd Place:";
                case 3:
                    return place + "rd Place:";
                default:
                    return place + "th Place:";
            }
        }

        [Command("distributepoints")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("distributepoints <channel>", "Distributes event points, based on the reactions on a user's message.")]
        public async Task DistributePointsPerRatings(ISocketMessageChannel channel)
        {
            string emoteName = _settings.Get("brackeys-emote").Split(':').First();

            Dictionary<IUser, int> userScoreLookup = new Dictionary<IUser, int>();

            var messages = await channel.GetMessagesAsync().Flatten();
            foreach (IMessage msg in messages)
            {
                var kvp = (msg as IUserMessage).Reactions.FirstOrDefault(r => r.Key.Name == emoteName);
                int score = kvp.Value.ReactionCount;

                if (userScoreLookup.ContainsKey(msg.Author))
                {
                    if (userScoreLookup[msg.Author] < score)
                        userScoreLookup[msg.Author] = score;
                }
                else userScoreLookup.Add(msg.Author, score);
            }

            var sortedPlaces = userScoreLookup.OrderByDescending(k => k.Value).ToArray();
            for (int i = 0; i < sortedPlaces.Length; i++)
            {
                var place = sortedPlaces[i];

                int points = 3;
                if (i < 3) points += (3 - i) * 3;

                _pointTable.AddPoints(place.Key, points);
            }

            await UpdateTopUsersWithRoles(true);
        }

        [Command("resetleaderboard")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("resetleaderboard", "This command resets the entire command and can't be undone.")]
        public async Task ResetLeaderboard()
        {
            _pointTable.Reset();
            await ReplyAsync("The leaderboard has been reset!");
        }

        /// <summary>
        /// Provides utility to navigate the pages of a leaderboard.
        /// </summary>
        public class LeaderboardNavigator
        {
            public class LeaderboardDisplayData
            {
                public int DisplayIndex { get; set; }
                public IUserMessage Message { get; set; }
                public IGuild Guild { get; set; }
            }

            private readonly EventPointTable _table;
            private readonly SettingsTable _settings;

            private readonly Dictionary<ulong, LeaderboardDisplayData> _trackedLeaderboards = new Dictionary<ulong, LeaderboardDisplayData>();

            public LeaderboardNavigator(EventPointTable table, SettingsTable settings)
            {
                _table = table;
                _settings = settings;
            }

            public void AddTrackedMessage(LeaderboardDisplayData messageData)
            {
                _trackedLeaderboards.Add(messageData.Message.Id, messageData);
            }
            public async Task HandleLeaderboardNavigation(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
            {
                // Check if the message is being tracked
                if (_trackedLeaderboards.ContainsKey(reaction.MessageId))
                {
                    // If so, gather the data for the tracked message
                    var data = _trackedLeaderboards[reaction.MessageId];
                    var message = data.Message;

                    int.TryParse(_settings["leaderboard-pagesize"], out int pagesize);

                    // Save the value the leaderboard should be modified by
                    int modification = 0;

                    // Check if there is an excess reaction for LEFT
                    var leftReactions = await message.GetReactionUsersAsync(LEFT_ARROW, 2);
                    if (leftReactions.Count() > 1)
                    {
                        // Navigate to the left and remove the reaction
                        modification = -pagesize;
                        await message.RemoveReactionAsync(new Emoji(LEFT_ARROW), leftReactions.First(u => !u.IsBot));
                    }
                    else
                    {
                        // Check if there is an excess reaction for RIGHT
                        var rightReactions = await message.GetReactionUsersAsync(RIGHT_ARROW, 2);
                        if (rightReactions.Count() > 1)
                        {
                            // Navigate to the right and remove the reaction
                            modification = +pagesize;
                            await message.RemoveReactionAsync(new Emoji(RIGHT_ARROW), rightReactions.First(u => !u.IsBot));
                        }
                    }

                    // If a change should be made, modify the leaderboard
                    if (modification != 0)
                    {
                        int min = 0, max = _table.RegisteredUsers - 1;

                        if (data.DisplayIndex + modification < min)
                        {
                            modification = min;
                        }
                        if (data.DisplayIndex + modification > max)
                        {
                            return;
                        }

                        data.DisplayIndex += modification;

                        // Build the new content and modify the message
                        Embed newContent = await BuildForNextPlaces(_table, data.Guild, data.DisplayIndex, pagesize);
                        await message.ModifyAsync(m => m.Embed = new Optional<Embed>(newContent));
                    }
                }
            }
        }
    }
}
