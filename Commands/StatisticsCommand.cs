using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    public class StatisticsCommand : ModuleBase
    {
        private readonly StatisticsTable _statisticsTable;
        private readonly SettingsTable _settings;
        private readonly StatisticsNavigator _navigator;

        private const string LEFT_ARROW = "⬅";
        private const string RIGHT_ARROW = "➡";

        public StatisticsCommand(StatisticsTable statisticsTable, SettingsTable settings, StatisticsNavigator navigator)
        {
            _statisticsTable = statisticsTable;
            _settings = settings;
            _navigator = navigator;
        }

        [Command("statistics")]
        [HelpData("statistics", "Displays the most frequently used commands.", HelpMode = "mod")]
        public async Task DisplayLeaderboard()
        {
            (Context.User as IGuildUser).EnsureStaff();

            int.TryParse(_settings["leaderboard-pagesize"], out int pagesize);

            var builder = await BuildForNextPlaces(_statisticsTable, Context.Guild, 0, pagesize);
            var message = await ReplyAsync(string.Empty, false, builder);

            await message.AddReactionAsync(new Emoji(LEFT_ARROW));
            await message.AddReactionAsync(new Emoji(RIGHT_ARROW));

            _navigator.AddTrackedMessage(new StatisticsNavigator.LeaderboardDisplayData() { DisplayIndex = 0, Message = message, Guild = Context.Guild });
        }

        private static async Task<EmbedBuilder> BuildForNextPlaces(StatisticsTable table, IGuild guild, int startIndex, int pagesize)
        {
            IEnumerable<KeyValuePair<string, uint>> places = table.GetLeaderboardPlaces(startIndex, pagesize);

            EmbedBuilder eb = new EmbedBuilder()
                .WithColor(new Color(0, 255, 255))
                .WithTitle("Command Statistics:");

            int count = places.Count();
            for(int i = 0; i < count; i++)
            {
                KeyValuePair<string, uint> place = places.ElementAt(i);

                string commandName = place.Key;

                string pointsDisplay = $"{ place.Value } use{ (place.Value != 1 ? "s" : "") }";

                string title = GetPlaceStringRepresentation(startIndex + i);
                string content = $"{ commandName } with { pointsDisplay }.";

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
            switch (place)
            {
                case 1: return "1st Place:";
                case 2: return "2nd Place:";
                case 3: return "3rd Place:";

                default: return place + "th Place:";
            }
        }

        public class StatisticsNavigator
        {
            public class LeaderboardDisplayData
            {
                public int DisplayIndex { get; set; }
                public IUserMessage Message { get; set; }
                public IGuild Guild { get; set; }
            }
            
            private readonly StatisticsTable _table;
            private readonly SettingsTable _settings;

            private readonly Dictionary<ulong, LeaderboardDisplayData> _trackedLeaderboards = new Dictionary<ulong, LeaderboardDisplayData>();

            public StatisticsNavigator (StatisticsTable table, SettingsTable settings)
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
                    if (leftReactions.Count > 1)
                    {
                        // Navigate to the left and remove the reaction
                        modification = -pagesize;
                        await message.RemoveReactionAsync(new Emoji(LEFT_ARROW), leftReactions.First(u => !u.IsBot));
                    }
                    else
                    {
                        // Check if there is an excess reaction for RIGHT
                        var rightReactions = await message.GetReactionUsersAsync(RIGHT_ARROW, 2);
                        if (rightReactions.Count > 1)
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
                            data.DisplayIndex = min;
                        }
                        if (data.DisplayIndex + modification > max)
                        {
                            return;
                        }

                        data.DisplayIndex += modification;

                        // Build the new content and modify the message
                        EmbedBuilder newContent = await BuildForNextPlaces(_table, data.Guild, data.DisplayIndex, pagesize);
                        await message.ModifyAsync(m => m.Embed = new Optional<Embed>(newContent));
                    }
                }
            }
        }
    }
}
