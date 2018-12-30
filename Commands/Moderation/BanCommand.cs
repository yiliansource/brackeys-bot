using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Discord.Commands;
using Discord;
using Discord.WebSocket;

namespace BrackeysBot.Commands.Moderation
{
    public class BanCommand : ModuleBase
    {
        private readonly BanTable _bans;

        public BanCommand(BanTable bans)
        {
            _bans = bans;
        }

        [Command("ban")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("ban <member> <reason> (optional)", "Ban a member.")]
        public async Task Ban(IGuildUser user, [Optional] [Remainder] string reason)
        {
            _bans.Remove(user.Id.ToString() + "," + Context.Guild.Id.ToString());
            string _displayName = user.GetDisplayName();
            await Context.Guild.AddBanAsync(user, 7, reason);

            IMessage messageToDel = await ReplyAsync($":white_check_mark: Successfully banned {_displayName}.");
            _ = messageToDel.TimedDeletion(3000);
            await Context.Message.DeleteAsync();
        }

        [Command("tempban")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("tempban <member> <duration in hours> <reason> (optional)", "Ban a member for the specified amount of time.")]
        public async Task Tempban(IGuildUser user, double duration, [Optional] [Remainder] string reason)
        {
            _bans.Set(user.Id.ToString() + "," + Context.Guild.Id.ToString(), (DateTime.UtcNow + new TimeSpan((long)(duration * TimeSpan.TicksPerHour))).ToBinary().ToString());
            string _displayName = user.GetDisplayName();

            await Context.Guild.AddBanAsync(user, 7, reason);

            IMessage messageToDel = await ReplyAsync($":white_check_mark: Successfully banned {_displayName} for {duration} hours.");
            _ = messageToDel.TimedDeletion(3000);
            await Context.Message.DeleteAsync();
        }

        [Command("unban")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("unban <name#discriminator OR user id>", "Unban a member.")]
        public async Task Unban([Remainder] string identification)
        {
            IUser user = null;
            ulong id;
            bool _idMode = false;
            string response = "User not found.";

            if (ulong.TryParse(identification, out id))
                _idMode = true;

            var bans = await Context.Guild.GetBansAsync();
            foreach (IBan ban in bans)
            {
                if (_idMode)
                {
                    if (ban.User.Id == id)
                    {
                        user = ban.User;
                        break;
                    }
                }
                else
                {
                    if ($"{ban.User.Username}#{ban.User.Discriminator}" == identification)
                    {
                        user = ban.User;
                        break;
                    }
                }
            }
            if (user != null)
            {
                response = $":white_check_mark: Successfully unbanned {user.Username}.";
                await Context.Guild.RemoveBanAsync(user);
                _bans.Remove(user.Id.ToString() + "," + Context.Guild.Id.ToString());
            }
            IMessage messageToDel = await ReplyAsync(response);
            _ = messageToDel.TimedDeletion(3000);
            await Context.Message.DeleteAsync();
        }
    }
}
