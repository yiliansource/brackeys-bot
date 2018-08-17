using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    public class LeaderboardDisplayData
    {
        public int DisplayIndex { get; set; }
        public IUserMessage Message { get; set; }
        public IGuild Guild { get; set; }
    }

    public class LeaderboardCommand : ModuleBase
    {
        private readonly CommandService commands;

        private const string LEFT_ARROW = "⬅";
        private const string RIGHT_ARROW = "➡";

        private static Dictionary<ulong, LeaderboardDisplayData> TrackedLeaderboards = new Dictionary<ulong, LeaderboardDisplayData>();

        private static int LeaderboardPagesize => int.Parse(BrackeysBot.Settings["leaderboard-pagesize"]);

        public LeaderboardCommand(CommandService commands)
        {
            this.commands = commands;
        }

        [Command("leaderboard")]
        public async Task DisplayLeaderboard()
        {
            var builder = await BuildForNextPlaces(0, Context.Guild);
            var message = await ReplyAsync(string.Empty, false, builder);

            await message.AddReactionAsync(new Emoji(LEFT_ARROW));
            await message.AddReactionAsync(new Emoji(RIGHT_ARROW));

            TrackedLeaderboards.Add(message.Id, new LeaderboardDisplayData() { DisplayIndex = 0, Message = message, Guild = Context.Guild });
        }

        private static async Task<EmbedBuilder> BuildForNextPlaces(int startIndex, IGuild guild)
        {
            KarmaTable table = BrackeysBot.Karma;
            IEnumerable<Tuple<ulong, int>> places = table.GetLeaderboardPlaces(startIndex, LeaderboardPagesize);

            EmbedBuilder eb = new EmbedBuilder()
                .WithColor(new Color(0, 255, 255))
                .WithTitle("Karma Leaderboard:");

            int count = places.Count();
            for(int i = 0; i < count; i++)
            {
                Tuple<ulong, int> place = places.ElementAt(i);

                IGuildUser user = await guild.GetUserAsync(place.Item1);

                string title = GetPlaceStringRepresentation(startIndex + i);
                string pointsDisplay = $"{ place.Item2 } point{ (place.Item2 != 1 ? "s" : "") }";
                string content = $"{ user.Username } with { pointsDisplay }.";

                eb.AddField(title, content);
            }

            return eb;
        }

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

        public static async Task HandleLeaderboardNavigation (Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            // Check if the message is being tracked
            if (TrackedLeaderboards.ContainsKey(reaction.MessageId))
            {
                // If so, gather the data for the tracked message
                var data = TrackedLeaderboards[reaction.MessageId];
                var message = data.Message;

                // Save the value the leaderboard should be modified by
                int modification = 0;

                // Check if there is an excess reaction for LEFT
                var leftReactions = await message.GetReactionUsersAsync(LEFT_ARROW, 2);
                if (leftReactions.Count > 1)
                {
                    // Navigate to the left and remove the reaction
                    modification = -LeaderboardPagesize;
                    await message.RemoveReactionAsync(new Emoji(LEFT_ARROW), leftReactions.First(u => !u.IsBot));
                }
                else
                {
                    // Check if there is an excess reaction for RIGHT
                    var rightReactions = await message.GetReactionUsersAsync(RIGHT_ARROW, 2);
                    if (rightReactions.Count > 1)
                    {
                        // Navigate to the right and remove the reaction
                        modification = +LeaderboardPagesize;
                        await message.RemoveReactionAsync(new Emoji(RIGHT_ARROW), rightReactions.First(u => !u.IsBot));
                    }
                }

                // If a change should be made, modify the leaderboard
                if (modification != 0)
                {
                    data.DisplayIndex += modification;

                    // Clamp the index so it cant be negative and cant exceed the total users
                    int min = 0, max = BrackeysBot.Karma.TotalLeaderboardUsers - 1;
                    data.DisplayIndex = Math.Clamp(data.DisplayIndex, min, max);

                    // Build the new content and modify the message
                    EmbedBuilder newContent = await BuildForNextPlaces(data.DisplayIndex, data.Guild);
                    await message.ModifyAsync(m => m.Embed = new Optional<Embed>(newContent));
                }
            }
        }
    }
}
