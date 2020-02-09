using System.Linq;
using System.Collections.Generic;

using Discord;
using Discord.WebSocket;

namespace BrackeysBot.Services
{
    public class EndorseService : BrackeysBotService
    {
        public struct EndorseEntry
        {
            public IUser User { get; set; }
            public int Stars { get; set; }
        }

        private readonly DataService _data;
        private readonly DiscordSocketClient _client;

        public EndorseService(DataService data, DiscordSocketClient client)
        {
            _data = data;
            _client = client;
        }

        public int GetUserStars(IUser user)
        {
            if (_data.UserData.TryGetUser(user.Id, out UserData data))
                return data.Stars;
            else
                return 0;
        }
        public void SetUserStars(IUser user, int amount)
        {
            UserData userData = _data.UserData.GetOrCreate(user.Id);
            userData.Stars = amount;

            _data.SaveUserData();
        }

        public EndorseEntry[] GetEndorseLeaderboard(int top = 10)
            => FilterValidEndorseUsers()
                .OrderByDescending(u => u.Stars)
                .Take(top)
                .Select(u => new EndorseEntry { User = _client.GetUser(u.ID), Stars = u.Stars })
                .ToArray();

        public int GetUserRank(IUser user)
            => GetUserRank(user, FilterValidEndorseUsers());

        private int GetUserRank(IUser user, IEnumerable<UserData> ofUsers)
            => ofUsers.OrderByDescending(u => u.Stars).ToList().FindIndex(u => u.ID == user.Id) + 1;

        public string FormatRank(IUser user)
        {
            var users = FilterValidEndorseUsers();
            int rank = GetUserRank(user, users);
            return rank == 0 ? string.Empty : $"Rank {rank} of {users.Count()}.";
        }

        private IEnumerable<UserData> FilterValidEndorseUsers()
            => _data.UserData.Users.Where(u => u.Stars > 0 && _client.GetUser(u.ID) != null);
    }
}