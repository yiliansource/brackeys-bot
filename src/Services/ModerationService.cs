using System;
using System.Collections.Generic;
using System.Text;

using Discord;

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

        public void RegisterTemporaryBan(IUser user, IUser moderator, DateTime unbanDate, string reason = "")
        {
            var userData = _data.UserData.GetOrCreate(user.Id);
            userData.TemporaryInfractions.Add(TemporaryInfraction.Create(TemporaryInfractionType.TempBan, unbanDate));
            userData.Infractions.Add(Infraction.Create(moderator.Id, $"[TempBan] {reason}"));

            _data.SaveUserData();
        }
    }
}
