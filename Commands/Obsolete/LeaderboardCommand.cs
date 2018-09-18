using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    class LeaderboardCommand : ModuleBase
    {
        private readonly KarmaTable _karmaTable;
        private readonly SettingsTable _settings;
        private readonly LeaderboardNavigator _navigator;

        private const string LEFT_ARROW = "⬅";
        private const string RIGHT_ARROW = "➡";

        public LeaderboardCommand(KarmaTable karmaTable, SettingsTable settings, LeaderboardNavigator navigator)
        {
            _karmaTable = karmaTable;
            _settings = settings;
            _navigator = navigator;
        }

        [Command("leaderboard")]
        [HelpData("leaderboard", "Displays the karma leaderboard.")]
        public async Task DisplayLeaderboard()
        {
            int.TryParse(_settings["leaderboard-pagesize"], out int pagesize);

            var builder = await BuildForNextPlaces(_karmaTable, Context.Guild, 0, pagesize);
            var message = await ReplyAsync(string.Empty, false, builder.Build());

            await message.AddReactionAsync(new Emoji(LEFT_ARROW));
            await message.AddReactionAsync(new Emoji(RIGHT_ARROW));

            _navigator.AddTrackedMessage(new LeaderboardNavigator.LeaderboardDisplayData() { DisplayIndex = 0, Message = message, Guild = Context.Guild });
        }

        private static async Task<EmbedBuilder> BuildForNextPlaces(KarmaTable table, IGuild guild, int startIndex, int pagesize)
        {
            IEnumerable<KeyValuePair<ulong, int>> places = table.GetLeaderboardPlaces(startIndex, pagesize);

            EmbedBuilder eb = new EmbedBuilder()
                .WithColor(new Color(0, 255, 255))
                .WithTitle("Karma Leaderboard:");

            int count = places.Count();
            for(int i = 0; i < count; i++)
            {
                KeyValuePair<ulong, int> place = places.ElementAt(i);

                IGuildUser user = await guild.GetUserAsync(place.Key);

                string pointsDisplay = $"{ place.Value } point{ (place.Value != 1 ? "s" : "") }";

                string title = GetPlaceStringRepresentation(startIndex + i);
                string content = $"{ UserHelper.GetDisplayName(user) } with { pointsDisplay }.";

                eb.AddField(title, content);
            }

            return eb;
        }

        /// <summary>
        /// Gets the string representation of a place, given its index. (0 -> "1st Place", 1 -> "2nd Place")
        /// </summary>
        private static string GetPlaceStringRepresentation (int placeIndex)
        {
            int place = placeIndex + 1;
            switch(place % 100)
            {
                case 11:
                case 12:
                case 13:
                    return place + "th Place:";
            }

            switch(place % 10)
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

        public class LeaderboardNavigator
        {
            public class LeaderboardDisplayData
            {
                public int DisplayIndex { get; set; }
                public IUserMessage Message { get; set; }
                public IGuild Guild { get; set; }
            }
            
            private readonly KarmaTable _table;
            private readonly SettingsTable _settings;

            private readonly Dictionary<ulong, LeaderboardDisplayData> _trackedLeaderboards = new Dictionary<ulong, LeaderboardDisplayData>();

            public LeaderboardNavigator (KarmaTable table, SettingsTable settings)
            {
                _table = table;
                _settings = settings;
            }
            
            public void AddTrackedMessage (LeaderboardDisplayData messageData)
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
                        int min = 0, max = _table.TotalLeaderboardUsers - 1;

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
                        EmbedBuilder newContent = await BuildForNextPlaces(_table, data.Guild, data.DisplayIndex, pagesize);
                        await message.ModifyAsync(m => m.Embed = new Optional<Embed>(newContent.Build()));
                    }
                }
            }
        }
    }
}
