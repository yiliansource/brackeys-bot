using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Discord;

using Humanizer;

namespace BrackeysBot.Services
{
    public class ModerationService : BrackeysBotService
    {
        private readonly DataService _data;

        public ModerationService(DataService data)
        {
            _data = data;
        }

        public void AddInfraction(IUser user, Infraction infraction)
        {
            var userData = _data.UserData.GetOrCreate(user.Id);
            userData.Infractions.Add(infraction);

            _data.SaveUserData();
        }

        public void AddTemporaryInfraction(TemporaryInfractionType type, IUser user, IUser moderator, TimeSpan duration, string reason = "")
        {
            var userData = _data.UserData.GetOrCreate(user.Id);
            userData.TemporaryInfractions.Add(TemporaryInfraction.Create(type, DateTime.UtcNow.Add(duration)));
            userData.Infractions.Add(Infraction.Create(RequestInfractionID())
                .WithType(type.AsInfractionType())
                .WithModerator(moderator)
                .WithDescription(reason)
                .WithAdditionalInfo($"Duration: {duration.Humanize(7)}"));

            _data.SaveUserData();
        }
        public void ClearTemporaryInfraction(TemporaryInfractionType type, IUser user)
            => ClearTemporaryInfraction(type, user.Id);
        public void ClearTemporaryInfraction(TemporaryInfractionType type, ulong userId)
        {
            var userData = _data.UserData.GetOrCreate(userId);
            userData.TemporaryInfractions.RemoveAll(i => i.Type == type);

            _data.SaveUserData();
        }

        public int ClearInfractions(IUser user)
        {
            if (_data.UserData.HasUser(user.Id))
            {
                UserData userData = _data.UserData.GetUser(user.Id);
                int infractionCount = userData.Infractions.Count;
                userData.Infractions.Clear();

                _data.SaveUserData();

                return infractionCount;
            }
            return 0;
        }
        public bool DeleteInfraction(int id)
        {
            if (TryGetInfraction(id, out Infraction _, out ulong userId))
            {
                _data.UserData.GetUser(userId).Infractions.RemoveAll(i => i.ID == id);
                _data.SaveUserData();
                return true;
            }
            return false;
        }

        public bool TryGetInfraction(int id, out Infraction infraction, out ulong userId)
        {
            UserData data = _data.UserData.Users.FirstOrDefault(u => u.Infractions.Any(i => i.ID == id));
            if (data != null)
            {
                infraction = data.Infractions.First(i => i.ID == id);
                userId = data.ID;
                return true;
            }
            else
            {
                infraction = default;
                userId = 0;
                return false;
            }
        }

        public int RequestInfractionID()
            => _data.UserData.Users.Count > 0
                ? 1 + _data.UserData.Users.Max(u => u.Infractions?.Count > 0 ? u.Infractions.Max(i => i.ID) : -1)
                : 1;
    }
}
