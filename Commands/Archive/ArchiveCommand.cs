using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using WeTransfer.NET;

namespace BrackeysBot.Commands.Archive
{
    public class ArchiveCommand : ModuleBase
    {
        // Detailed format here:
        //   https://github.com/YilianSource/brackeys-bot/issues/138

        private readonly SettingsTable _settings;

        public ArchiveCommand(SettingsTable settings)
        {
            _settings = settings;
        }

        [Command("archive")]
        [PermissionRestriction(UserType.Staff)]
        public async Task ArchiveChannel(ISocketMessageChannel channel)
            => await ArchiveChannel(channel, channel.Name).ConfigureAwait(false);

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

                WeTransferClient wt = new WeTransferClient(BrackeysBot.Configuration["wetransfer-api-key"]);
                // Make sure we are authorized
                await wt.Authorize();

                var fileInfo = new System.IO.FileInfo(zippedArchive);
                // Create Partial File Information so WeTransfer knows how many files
                // you're going to upload, the names of those files and their sizes
                PartialFileInfo[] partialFileInfos = new PartialFileInfo[]
                {
                    new PartialFileInfo
                    {
                        Name = fileInfo.Name,
                        Path = fileInfo.FullName,
                        Size = (int)fileInfo.Length
                    }
                };

                // Create a File Transfer which informs WeTransfer that you're about to upload files
                // The second parameter is the transfer message which will show on the download page
                FileTransferResponse response = await wt.CreateTransfer(partialFileInfos, $"Download the archived channel #{channel.Name}!");

                // Now you can upload the files!
                // The first parameter is the transfer's ID
                await wt.Upload(response.ID, response.Files);

                // Now you need to tell WeTransfer that your files have been uploaded
                FileUploadResult result = await wt.FinalizeUpload(response.ID, response.Files);

                // FileUploadResult contains the url to the download page and the date of the expiry

                StringBuilder reply = new StringBuilder()
                    .AppendLine($"I archived the channel <#{channel.Id}> for you{(title == channel.Name ? "!" : $", under the name **{title}**!")}")
                    .AppendLine($"You can download it from <{result.URL}> for {Math.Ceiling((result.ExpiresAt - DateTime.UtcNow).TotalDays).ToString("0")} days.")
                    .AppendLine()
                    .AppendLine($"The archive contains **{archive.ArchivedMessages} messages** and **{archive.ArchivedImages} images**.");
                await Context.Channel.SendMessageAsync(reply.ToString());
            }
        }
    }
}
