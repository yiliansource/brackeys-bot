using System;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

using Discord;
using Discord.WebSocket;

namespace BrackeysBot.Services
{
    public class TemporaryInfractionService : BrackeysBotService
    {
        private readonly DataService _data;
        private readonly DiscordSocketClient _client;
        private readonly LoggingService _log;
        private readonly ModerationLogService _modLog;

        private readonly Timer _checkTimer;

        public TemporaryInfractionService(DataService data, DiscordSocketClient client, LoggingService log, ModerationLogService modLog)
        {
            _data = data;
            _client = client;
            _log = log;
            _modLog = modLog;

            _checkTimer = new Timer(TimeSpan.FromSeconds(20).TotalMilliseconds)
            {
                AutoReset = true,
                Enabled = true
            };
            _checkTimer.Elapsed += (s, e) => CheckTemporaryInfractions();
            _checkTimer.Start();
        }

        private void CheckTemporaryInfractions()
        {
            DateTime now = DateTime.Now;
            SocketGuild guild = _client.GetGuild(_data.Configuration.Guild);

            int resolvedCounter = 0;

            foreach (UserData user in _data.UserData.GetUsersWithTemporalInfractions())
            {
                if (user.TemporaryInfractions.Any(t => t.Type == TemporaryInfractionType.TempBan))
                {
                    TemporaryInfraction infraction = user.TemporaryInfractions.First(t => t.Type == TemporaryInfractionType.TempBan);
                    if (infraction.Expire <= now)
                    {
                        _ = guild.RemoveBanAsync(user.ID);
                        _modLog.CreateEntry(ModerationLogEntry.New
                            .WithActionType(ModerationActionType.Unban)
                            .WithTarget(user.ID)
                            .WithReason("Temporary ban timed out.")
                            .WithTime(DateTimeOffset.Now)
                            .WithModerator(_client.CurrentUser));

                        user.TemporaryInfractions.RemoveAll(i => i.Type == TemporaryInfractionType.TempBan);
                        resolvedCounter++;
                    }
                }
                // TODO: Unmute
            }

            if (resolvedCounter > 0)
            {
                _log.LogMessageAsync(new LogMessage(LogSeverity.Info, "TemporaryInfractions", $"Resolved {resolvedCounter} temporary infraction(s)."));
                _data.SaveUserData();
            }
        }
    }
}
