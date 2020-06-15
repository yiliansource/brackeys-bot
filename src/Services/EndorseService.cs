using System.Linq;
using System.Collections.Generic;

using Discord;
using Discord.WebSocket;
using System;

namespace BrackeysBot.Services
{
    public class EndorseService : BrackeysBotService
    {
        public struct EndorseEntry
        {
            public IUser User { get; set; }
            public int Stars { get; set; }
        }

        private readonly Dictionary<ulong, int> _lastEndorsements;
        private readonly DataService _data;
        private readonly DiscordSocketClient _client;

        public EndorseService(DataService data, DiscordSocketClient client)
        {
            _data = data;
            _client = client;
            _lastEndorsements = new Dictionary<ulong, int>();
        }

        public int GetUserStars(IUser user)
        {
            return 0;
        }
    }
}