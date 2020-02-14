using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Humanizer;
using Humanizer.Localisation;

namespace BrackeysBot.Commands
{
    public partial class UtilityModule : BrackeysBotModule
    {
        [Command("timeleft"), Alias("gamejamtime")]
        [Summary("Displays when the jam starts or ends.")]
        public async Task ShowGamejamTimeLeftAsync()
        {
            EmbedBuilder reply = GetDefaultBuilder();

            if (Data.Configuration.GamejamTimestamps?.Length == 0)
            {
                reply.WithDescription("No jams are currently scheduled!");
            }
            else
            {
                DateTimeOffset[] times = Data.Configuration.GamejamTimestamps.Select(t => DateTimeOffset.FromUnixTimeSeconds(t)).ToArray();
                DateTimeOffset now = DateTimeOffset.UtcNow;

                bool CheckTime(DateTimeOffset time, string message)
                {
                    if (now < time)
                    {
                        TimeSpan diff = time - now;
                        reply.WithDescription(string.Format(message, diff.Humanize(7, minUnit: TimeUnit.Second)));

                        return true;
                    }
                    return false;
                }

                if (!CheckTime(times[0], "The jam will start in {0}."))
                    if (!CheckTime(times[1], "The jam has started! It will end in {0}."))
                        if (!CheckTime(times[2], "The jam has ended! The voting period will end in {0}."))
                            reply.WithDescription("No jams are currently scheduled!");
            }

            await reply.Build().SendToChannel(Context.Channel);
        }

        [Command("settimeleft")]
        [RequireModerator]
        public async Task SetGamejamTimesAsync(params string[] dates)
        {
            const string format = "dd/MM/yyyy-HH:mm:ss";

            if (dates.Length != 3)
                throw new ArgumentException($"Invalid time configuration. (Got {dates.Length}, expected 3)");

            Data.Configuration.GamejamTimestamps = dates.Select(t =>
            {
                if (!DateTimeOffset.TryParseExact(t, format, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out DateTimeOffset result))
                    throw new ArgumentException($"The date _{t}_ does not meet the required format of _{format}.");
                return result.ToUnixTimeSeconds();
            }).ToArray();
            Data.SaveConfiguration();

            var configDates = Data.Configuration.GamejamTimestamps.Select(l => DateTimeOffset.FromUnixTimeSeconds(l)).ToArray();
            await GetDefaultBuilder()
                .WithTitle("Gamejam times set!")
                .WithDescription($"The current configuration is that the jam: \n" +
                    $"... begins on {configDates[0].ToDateString()}. \n" +
                    $"... ends on {configDates[1].ToDateString()}. \n" +
                    $"... has the voting closed at {configDates[2].ToDateString()}.")
                .Build()
                .SendToChannel(Context.Channel);
        }
    }
}
