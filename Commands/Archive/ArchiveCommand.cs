using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace BrackeysBot.Commands.Archive
{
    public class ArchiveCommand : ModuleBase
    {
        [Serializable]
        protected class TransferPostData
        {
            [Serializable]
            public class File
            {
                [JsonProperty("name")]
                public string Name { get; set; }
                [JsonProperty("size")]
                public long Size { get; set; }
            }

            [JsonProperty("message")]
            public string Message { get; set; }
            [JsonProperty("files")]
            public File[] Files { get; set; }
        }
        [Serializable]
        protected class TransferResponse
        {
            [Serializable]
            public class File
            {
                [Serializable]
                public class MultipartInfo
                {
                    [JsonProperty("part_numbers")]
                    public int PartNumbers { get; set; }
                    [JsonProperty("chunk_size")]
                    public int ChunkSize { get; set; }
                }

                [JsonProperty("multipart")]
                public MultipartInfo Multipart { get; set; }
                [JsonProperty("size")]
                public long Size { get; set; }
                [JsonProperty("type")]
                public string Type { get; set; }
                [JsonProperty("name")]
                public string Name { get; set; }
                [JsonProperty("id")]
                public string ID { get; set; }
            }

            [JsonProperty("success")]
            public bool Success { get; set; }
            [JsonProperty("id")]
            public string ID { get; set; }
            [JsonProperty("message")]
            public string Message { get; set; }
            [JsonProperty("state")]
            public string State { get; set; }
            [JsonProperty("url")]
            public string Url { get; set; }
            [JsonProperty("expires_at")]
            public DateTime ExpiresAt { get; set; }
            [JsonProperty("files")]
            public File[] Files { get; set; }
        }

        // Detailed format here:
        //   https://github.com/YilianSource/brackeys-bot/issues/138

        private readonly SettingsTable _settings;

        private const string API_BASEPOINT = "https://dev.wetransfer.com/v2/";

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
                string archiveUrl = await UploadArchive(archive);

                StringBuilder reply = new StringBuilder()
                    .AppendLine($"I archived the channel <#{channel.Id}> for you{(title == channel.Name ? "!" : $", under the name **{title}**!")}")
                    .AppendLine($"You can download it from <{archiveUrl}>.")
                    .AppendLine()
                    .AppendLine($"The archive contains **{archive.ArchivedMessages} messages** and **{archive.ArchivedImages} images**.");
                await Context.Channel.SendMessageAsync(reply.ToString());
            }
        }

        private async Task<string> UploadArchive(ChannelArchive archive)
        {
            string zippedArchive = archive.ZipArchive();

            TransferPostData postData = new TransferPostData
            {
                Message = archive.Title,
                Files = new TransferPostData.File[]
                {
                    new TransferPostData.File
                    {
                        Name = archive.Title,
                        Size = new FileInfo(zippedArchive).Length
                    }
                }
            };

            // Create the client that will be used for all the further requests
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(API_BASEPOINT); // direct it to the api
            httpClient.DefaultRequestHeaders.Add("x-api-key", BrackeysBot.Configuration["wetransfer-api-key"]); // provide it with the api key
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); // make sure it accepts application/json
            httpClient.DefaultRequestHeaders.Connection.Add(HttpMethod.Post.ToString().ToUpper());

            // Post the authorization message. From that we will retrieve a token that is required for our further requests
            HttpResponseMessage authorizationResponseMessage = await httpClient.PostAsync("authorize", null);
            authorizationResponseMessage.EnsureSuccessStatusCode();
            string authorizationResponseString = await authorizationResponseMessage.Content.ReadAsStringAsync();
            string authToken = JObject.Parse(authorizationResponseString).Property("token").Value.ToString();

            // Add the token to the authorization
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Register a new transfer
            StringContent content = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");
            HttpResponseMessage transferResponseMessage = await httpClient.PostAsync("transfers", content);
            transferResponseMessage.EnsureSuccessStatusCode();
            string json = await transferResponseMessage.Content.ReadAsStringAsync();
            TransferResponse transferResponse = JsonConvert.DeserializeObject<TransferResponse>(json);

            // Upload the file
            TransferResponse.File transferFile = transferResponse.Files[0];
            int chunkSize = transferFile.Multipart.ChunkSize;
            using (Stream source = File.OpenRead(zippedArchive))
            {
                byte[] chunk = new byte[chunkSize];
                for (int i = 0; i < transferFile.Multipart.PartNumbers; i++)
                {
                    int partId = i + 1; // +1 because the API part IDs are 1-based
                    string requestUrl = $"transfers/{transferResponse.ID}/files/{transferFile.ID}/upload-url/{partId}";
                    HttpResponseMessage requestUrlResponse = await httpClient.GetAsync(requestUrl);
                    string requestUrlResponseString = await requestUrlResponse.Content.ReadAsStringAsync();
                    string partUrl = JObject.Parse(requestUrlResponseString).Property("url").Value.ToString();

                    await source.ReadAsync(chunk, i * chunkSize, chunkSize);

                    HttpWebRequest httpRequest = WebRequest.Create(partUrl) as HttpWebRequest;
                    httpRequest.Method = "PUT";
                    using (Stream dataStream = httpRequest.GetRequestStream())
                    {
                        dataStream.Write(chunk, 0, chunkSize);
                    }
                    HttpWebResponse response = httpRequest.GetResponse() as HttpWebResponse;
                    Console.WriteLine(response.StatusCode);

                    //ByteArrayContent chunkContent = new ByteArrayContent(chunk);
                    //HttpResponseMessage uploadResponseMessage = await httpClient.PostAsync(partUrl, chunkContent);
                    //uploadResponseMessage.EnsureSuccessStatusCode();
                }

                // Finalize the archive file
                HttpResponseMessage fileFinalizeResponseMessage = await httpClient.PutAsync($"transfers/{transferResponse.ID}/files/{transferFile.ID}/upload-complete", new StringContent(string.Empty, Encoding.UTF8, "application/json"));
                fileFinalizeResponseMessage.EnsureSuccessStatusCode();
            }

            // Finalize the transfer
            HttpResponseMessage finalTransferResponseMessage = await httpClient.PutAsync($"transfers/{transferResponse.ID}/finalize", new StringContent(string.Empty, Encoding.UTF8, "application/json"));
            finalTransferResponseMessage.EnsureSuccessStatusCode();
            string finalTransferResponseString = await finalTransferResponseMessage.Content.ReadAsStringAsync();
            TransferResponse finalTransferResponse = JsonConvert.DeserializeObject<TransferResponse>(finalTransferResponseString);

            return finalTransferResponse.Url;
        }
    }
}
