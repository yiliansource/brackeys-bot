using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Humanizer;
using Humanizer.Localisation;

namespace BrackeysBot.Commands
{
    public partial class UtilityModule : BrackeysBotModule
    {
        [Command("collaboration"), Alias("collab")]
        [Summary("Starts building an embed to be sent into the collaboration channels. Check your direct messages after using the command.")]
        public async Task StartCollabDialogueAsync()
        {
            bool canOverride = (Context.User as IGuildUser).GetPermissionLevel(Context) >= PermissionLevel.Moderator;
            int remaining = CollabService.CollabTimeoutRemaining(Context.User);

            if (!canOverride && remaining > 0)
                throw new TimeoutException($"You need to wait {TimeSpan.FromMilliseconds(remaining).Humanize(2, minUnit: TimeUnit.Second)} before you can use this command again!");

            // Does the user already have an active conversation
            if (CollabService.TrySetActiveUser(Context.User))
            {
                await Context.Channel.SendMessageAsync("Please check your DMs. If you want to cancel or restart the process, use the command again.");

                string message = $"Hello **{Context.User.Username}!**\nPlease enter which channel you would like to post:\n1- Paid\n2- Hobby\n3- Gametest\n4- Mentor";
                await Context.User.TrySendMessageAsync(message);
            }
            else
            {
                CollabService.DeactivateUser(Context.User);
                await Context.Channel.SendMessageAsync("Process cancelled. You can run the command again to restart the process.");
            }          
        }	
    }
}
