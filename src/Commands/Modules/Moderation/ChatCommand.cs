using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Humanizer;

namespace BrackeysBot.Commands
{
    public partial class ModerationModule : BrackeysBotModule
    {
        private readonly int _maxSlowMode = 21600;

        [Command("slowmode"), Alias("slow")]
        [Summary("Sets slowmode in the current channel")]
        [Remarks("slowmode <duration>")]
        [RequireModerator]
        [RequireContext(ContextType.Guild)]
        public async Task SlowModeAsync(
            [Summary("The duration of slowmode"), OverrideTypeReader(typeof(AbbreviatedTimeSpanTypeReader))] TimeSpan duration)
        {
            EmbedBuilder builder = new EmbedBuilder();
            int slowmodeDuration = (int) duration.TotalSeconds;

            if (slowmodeDuration > _maxSlowMode || slowmodeDuration < 0) {
                builder.WithColor(Color.Red)
                    .WithDescription($"Duration ({duration.Humanize(3)}) out of bounds (0 - {_maxSlowMode / 3600} hours)!");
            } else {
                builder.WithColor(Color.Green);

                if (slowmodeDuration == 0) {
                    builder.WithDescription($"Slowmode is turned off!");
                } else {
                    builder.WithDescription($"Slowmode is set to {duration.Humanize(3)}!");
                }

                ITextChannel channel = await Context.Guild.GetTextChannelAsync(Context.Channel.Id);

                await channel.ModifyAsync(x => x.SlowModeInterval = slowmodeDuration);

                await ModerationLog.CreateEntry(ModerationLogEntry.New
                    .WithDefaultsFromContext(Context)
                    .WithActionType(ModerationActionType.SlowMode)
                    .WithDuration(duration));
            }

            await builder.Build().SendToChannel(Context.Channel);
        }
    }
}
