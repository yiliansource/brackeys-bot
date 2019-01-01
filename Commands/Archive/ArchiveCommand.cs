using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Commands.Archive
{
    public class ArchiveCommand : ModuleBase
    {
        // Detailed format here:
        //   https://github.com/YilianSource/brackeys-bot/issues/138

        private SettingsTable _settings;

        public ArchiveCommand(SettingsTable settings)
        {
            _settings = settings;
        }

        [Command("archive")]
        [PermissionRestriction(UserType.Staff)]
        public async Task ArchiveChannel(ISocketMessageChannel channel)
            => await ArchiveChannel(channel, channel.Name);

        [Command("archive")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("archive <channel> <title>", "Archives the given channel.")]
        public async Task ArchiveChannel(ISocketMessageChannel channel, [Remainder]string title)
        {
            var messages = await channel.GetMessagesAsync().Flatten();

            using (ChannelArchive archive = new ChannelArchive(title))
            {
                string voteEmote = _settings.Has("brackeys-emote") ? _settings.Get("brackeys-emote").Split(':').First() : string.Empty;

                foreach (IMessage msg in messages)
                {
                    ArchiveMessage archiveMessage = new ArchiveEventSubmission(msg, voteEmote);
                    IEnumerable<ArchiveImage> archiveImages = archiveMessage.GetMessageImages();

                    archive.AddMessage(archiveMessage);
                    foreach (ArchiveImage image in archiveImages)
                        archive.AddImage(image);
                }

                archive.CloseArchive();
                string zippedArchive = archive.ZipArchive();

                string reply = $"I archived the channel <#{channel.Id}> for you{(title == channel.Name ? "!" : $", under the name **{title}**!\nThe archive contains {archive.ArchivedMessages} messages and {archive.ArchivedImages} images.")}";
                IUserMessage message = await Context.Channel.SendFileAsync(zippedArchive, reply);
            }
        }
    }
}
