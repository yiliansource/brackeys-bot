using System;
using System.Linq;
using System.Threading.Tasks;
using BrackeysBot.Core.Models;
using BrackeysBot.Models.Database;
using BrackeysBot.Services;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace BrackeysBot.Managers
{
    public class ChatManager : BrackeysBotService
    {
        private readonly DiscordSocketClient _client;
        private readonly BotConfiguration _config;
        private readonly DatabaseService _db;
        private readonly LoggingService _logging;
        private readonly ModerationLogService _moderationLog;

        public ChatManager(DiscordSocketClient client, DataService data, DatabaseService db, LoggingService logging, ModerationLogService moderationLog) 
        {
            _client = client;
            _config = data.Configuration;
            _db = db;
            _logging = logging;
            _moderationLog = moderationLog;

            client.MessageDeleted += MessageDeletedHandler;
        }

        private async Task MessageDeletedHandler(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            IMessage msg = await message.GetOrDownloadAsync();
            GuildUserProxy proxy = _client.TryGetUser(_config.GuildID, msg.Author.Id);
            LoggedMessage loggedMessage = new LoggedMessage {
                DiscordChannelId = channel.Id,
                DiscordMessageId = message.Id,
                MsgAction = (int) MessageActionType.Deleted,
                UserId = proxy.ID,
                MsgContent1 = msg.Content
            };

            if (proxy.HasValue) 
            {
                IBan bannedUser = await _client.GetGuild(_config.GuildID).GetBanAsync(proxy.GuildUser);
                if (bannedUser != null)
                {
                    Infraction infr = _db.GetAllInfractionsOfUser(proxy.ID).FirstOrDefault(x => x.ModerationTypeId == (int) InfractionType.Ban || x.ModerationTypeId == (int) InfractionType.TemporaryBan);
                    _db.StoreMessage(loggedMessage, infr);
                }
            }
            else
                _db.StoreMessage(loggedMessage);
        }
    }
}