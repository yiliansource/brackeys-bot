using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands
{
    public class TimeLeftCommand : ModuleBase
    {
        private readonly SettingsTable _settings;

        private const string DATEFORMAT = "dd-MM-yyyy H:mm";

        public TimeLeftCommand(SettingsTable settings)
        {
            _settings = settings;
        }

        [Command("timeleft")]
        [HelpData("timeleft", "Displays the time until the next jam event takes place.")]
        public async Task JamTimeLeftCommand ()
        {
            if (!_settings.Has("jamevents"))
            {
                throw new Exception("No jam event dates have been set.");
            }

            // Ordered by:
            // [0]: Jam Start
            // [1]: Jam End
            // [2]: Voting End
            string jamEventDates = _settings.Get("jamevents");
            var dateStrings = jamEventDates.Split(',').Select(d => d.Trim());

            DateTime[] dates = dateStrings.Select(date => DateTime.ParseExact(date, DATEFORMAT, null, System.Globalization.DateTimeStyles.None)).ToArray();
            DateTime utcNow = DateTime.UtcNow;
            
            if (dates.Length != 3)
            {
                throw new Exception("Invalid jam date configuration. Please check the settings.");
            }

            if (dates[0] > utcNow)
            {
                string until = ConvertTimespanToString(dates[0] - utcNow);
                await ReplyAsync($"The jam will start in { until }.");
            }
            else if (dates[1] > utcNow)
            {
                string remaining = ConvertTimespanToString(dates[1] - utcNow);
                await ReplyAsync($"The jam has started! There are { remaining } remaining");
            }
            else if (dates[2] > utcNow)
            {
                string remaining = ConvertTimespanToString(dates[2] - utcNow);
                await ReplyAsync($"The jam has ended. There are { remaining } of the voting phase remaining.");
            }
            else
            {
                await ReplyAsync("The voting phase has ended! There are no jam events scheduled at the moment.");
            }
        }

        /// <summary>
        /// Converts a timespan to a string in the format "X days, X hours and X minutes.
        /// </summary>
        private static string ConvertTimespanToString (TimeSpan span)
        {
            int days = (int)Math.Floor(span.TotalDays);
            string dayDisplay = $"{days} day{ (days != 1 ? "s" : "") }";
            string hourDisplay = $"{span.Hours} hour{ (span.Hours != 1 ? "s" : "") }";
            string minuteDisplay = $"{span.Minutes} minute{ (span.Minutes != 1 ? "s" : "") }";

            return $"{ (days > 0 ? dayDisplay + ", " : "") }{hourDisplay} and {minuteDisplay}";
        }
    }
}
