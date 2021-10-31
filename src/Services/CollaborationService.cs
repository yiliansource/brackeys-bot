using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace BrackeysBot.Services
{
    public class CollaborationService : BrackeysBotService
    {
        
        private readonly DataService _data;
        private readonly DiscordSocketClient _client;
        private readonly ConcurrentDictionary<ulong, int> _lastCollabUsages = new ConcurrentDictionary<ulong, int>();
        private readonly ConcurrentDictionary<ulong, CollabConversation> _activeConversations = new ConcurrentDictionary<ulong, CollabConversation>();

        /// <inheritdoc />
        public CollaborationService(DataService data, DiscordSocketClient client)
        {
            _data = data;
            _client = client;
        }

        public void UpdateCollabTimeout(IUser user)
        {
            _lastCollabUsages.TryRemove(user.Id, out _);
            _lastCollabUsages.TryAdd(user.Id, Environment.TickCount);
        }
        
        public int CollabTimeoutRemaining(IUser user) 
        {
            int now = Environment.TickCount;
            int result = 0;

            if (_lastCollabUsages.TryGetValue(user.Id, out int last))
            {
                int passed = now - last;
                result = _data.Configuration.CollabTimeoutMillis - passed;

                if (result < 0) 
                    result = 0;
            }

            return result;
        }

        public bool TrySetActiveUser(IUser user)
        {
            if (!_activeConversations.ContainsKey(user.Id))
            {
                CollabConversation _conversation = new CollabConversation(_client, _data, this);
                _activeConversations.TryAdd(user.Id, _conversation);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsActiveUser(IUser user) 
            => _activeConversations.ContainsKey(user.Id);

        public void DeactivateUser(IUser user) 
            => _activeConversations.TryRemove(user.Id, out _);

        public async Task Converse(SocketUserMessage message)
        {
            ulong userId = message.Author.Id;
            _activeConversations[userId].UpdateMessage(message);
            await _activeConversations[userId].HandleAnswer();
        }

        private class CollabConversation
        {
            private readonly DiscordSocketClient _client;
            private readonly DataService _data;
            private readonly CollaborationService _collab;

            public CollabConversation(DiscordSocketClient client, DataService data, CollaborationService collab)
            {
                _client = client;
                _data = data;
                _collab = collab;
            }

            private enum CollabChannel
            {
                Unknown,
                Paid,
                Hobby,
                Gametest,
                Mentor
            }
            private enum HiringStatus
            {
                Unknown,
                NotHiring,
                Hiring
            }

            private SocketUserMessage _message;
            
            private int _buildStage = 0;
            private CollabChannel _collabChannel = CollabChannel.Unknown;
            private HiringStatus _hiring = HiringStatus.Unknown;
            private string _projectName;
            private string _roles;
            private string _skills;
            private string _portfolio;
            private string _areasOfInterest;
            private string _platforms;
            private string _description;
            private string _compensation;
            private string _teamSize;
            private string _experience;
            private string _projectLength;
            private string _responsibilities;
            private string _link;
            
            public void UpdateMessage(SocketUserMessage message)
                => _message = message;
            public async Task HandleAnswer()
            {
                if (_message == null)
                    return;

                string uppercaseMessage = _message.Content.ToUpper();

                // Channel Select
                if (_collabChannel == CollabChannel.Unknown)
                {
                    if (uppercaseMessage.Contains("PAID") || _message.Content == "1")
                    {
                        _collabChannel = CollabChannel.Paid;
                        await _message.Author.TrySendMessageAsync("Selected Channel: **Paid**.\nAre you looking for **work** or looking to **hire**?");
                    }
                    else if (uppercaseMessage.Contains("HOBBY") || _message.Content == "2")
                    {
                        _collabChannel = CollabChannel.Hobby;
                        await _message.Author.TrySendMessageAsync("Selected Channel: **Hobby**.\nAre you looking for a **team** or looking for **people**?");
                    }
                    else if (uppercaseMessage.Contains("GAMETEST") || _message.Content == "3")
                    {
                        _collabChannel = CollabChannel.Gametest;
                        await _message.Author.TrySendMessageAsync("Selected Channel: **Gametest**.\nWhat is the name of your project?");
                    }
                    else if (uppercaseMessage.Contains("MENTOR") || _message.Content == "4")
                    {
                        _collabChannel = CollabChannel.Mentor;
                        await _message.Author.TrySendMessageAsync("Selected Channel: **Mentor**.\nAre you looking **for** a mentor or looking **to** mentor people?");
                    }
                    else
                    {
                        await _message.Author.TrySendMessageAsync("**Invalid answer!**\nPlease enter which channel you would like to post:\n1- Paid\n2- Hobby\n3- Gametest\n4- Mentor");
                    }
                }
                else if (_collabChannel == CollabChannel.Paid)
                {
                    await HandlePaidAnswer(uppercaseMessage);
                }
                else if (_collabChannel == CollabChannel.Hobby)
                {
                    await HandleHobbyAnswer(uppercaseMessage);
                }
                else if (_collabChannel == CollabChannel.Gametest)
                {
                    await HandleGametestAnswer();
                }
                else if (_collabChannel == CollabChannel.Mentor)
                {
                    await HandleMentorAnswer(uppercaseMessage);
                }
            }

            private async Task HandlePaidAnswer(string uppercaseMessage)
            {
                if (_hiring == HiringStatus.Unknown)
                {
                    if (uppercaseMessage.Contains("WORK"))
                    {
                        _hiring = HiringStatus.NotHiring;
                        await _message.Author.TrySendMessageAsync("Selected: **Looking for work**.\nWhat is/are your role(s)?");
                    }
                    else if (uppercaseMessage.Contains("HIRE"))
                    {
                        _hiring = HiringStatus.Hiring;
                        await _message.Author.TrySendMessageAsync("Selected: **Looking to hire**.\nWhat is the name of your project?");
                    }
                    else
                    {
                        await _message.Author.TrySendMessageAsync("Invalid input.\nAre you looking for\n**- Work**\nOr are you looking to\n**- Hire**?");
                    }
                }
                else if (_hiring == HiringStatus.NotHiring)
                {
                    await HandleNotHiring();
                }
                else if (_hiring == HiringStatus.Hiring)
                {
                    await HandleHiring();
                }

                async Task HandleNotHiring()
                {
                    switch (_buildStage)
                    {
                        case 0:
                            _roles = _message.Content;
                            await _message.Author.TrySendMessageAsync("Which specific skills do you have? (*Ie. Unity, C#, Photoshop, Microsoft Excel etc.*)");
                            _buildStage++;
                            break;

                        case 1:
                            _skills = _message.Content;
                            await _message.Author.TrySendMessageAsync("Please list any previous projects or portfolio if you have one. (*N/A if none*)");
                            _buildStage++;
                            break;

                        case 2:
                            _portfolio = _message.Content;
                            await _message.Author.TrySendMessageAsync("How much experience do you have in the field? (*Ie. 2 Months, 5 Years etc.*)");
                            _buildStage++;
                            break;

                        case 3:
                            _experience = _message.Content;
                            await _message.Author.TrySendMessageAsync("Add a description. (Optional)");
                            _buildStage++;
                            break;

                        case 4:
                            _description = _message.Content;
                            await _message.Author.TrySendMessageAsync("How much are your rates ? (*Ie. \"**5$/work done**\", \"**5€/h**\", \"**10$/h ~ 30$/h**\"*)");
                            _buildStage++;
                            break;

                        case 5:
                            _compensation = _message.Content;
                            await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the paid channel");
                            FinalizeQuestionnaire();
                            await BuildPaidNotHiringEmbed();
                            break;
                    }
                }
                async Task HandleHiring()
                {
                    switch (_buildStage)
                    {
                        case 0:
                            _projectName = _message.Content;
                            await _message.Author.TrySendMessageAsync("Describe your project.");
                            _buildStage++;
                            break;

                        case 1:
                            _description = _message.Content;
                            await _message.Author.TrySendMessageAsync("Which roles are you looking to hire?");
                            _buildStage++;
                            break;

                        case 2:
                            _roles = _message.Content;
                            await _message.Author.TrySendMessageAsync("Please list any previous projects or portfolio if you have one. (**N/A** *if none*)");
                            _buildStage++;
                            break;

                        case 3:
                            _portfolio = _message.Content;
                            await _message.Author.TrySendMessageAsync("What is the current team size?");
                            _buildStage++;
                            break;

                        case 4:
                            _teamSize = _message.Content;
                            await _message.Author.TrySendMessageAsync("What is the project length? (specify if not strict)");
                            _buildStage++;
                            break;

                        case 5:
                            _projectLength = _message.Content;
                            await _message.Author.TrySendMessageAsync("What is the compensation? (*Ie. \"**5$/work done**\", \"**5€/h**\", \"**10$/h ~ 30$/h**\"*)");
                            _buildStage++;
                            break;

                        case 6:
                            _compensation = _message.Content;
                            await _message.Author.TrySendMessageAsync("What specific responsibilities will the person being hired will have? (*Ie. Implementing physics system, writing character backstories etc.*)");
                            _buildStage++;
                            break;

                        case 7:
                            _responsibilities = _message.Content;
                            await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the paid channel");
                            FinalizeQuestionnaire();
                            await BuildPaidHiringEmbed();
                            break;
                    }
                }
            }              
            private async Task HandleHobbyAnswer(string uppercaseMessage)
            {
                if (_hiring == HiringStatus.Unknown)
                {
                    if (uppercaseMessage.Contains("TEAM"))
                    {
                        _hiring = HiringStatus.NotHiring;
                        await _message.Author.TrySendMessageAsync("Selected: **Looking for a team**.\nWhat is/are your role(s)?");
                    }
                    else if (uppercaseMessage.Contains("PEOPLE"))
                    {
                        _hiring = HiringStatus.Hiring;
                        await _message.Author.TrySendMessageAsync("Selected: **Looking for people**.\nWhat is the name of your project?");
                    }
                    else
                    {
                        await _message.Author.TrySendMessageAsync("Invalid input.\nAre you looking for a \n**- Team**\nOr are you looking for\n**- People**?");
                    }
                }
                else if (_hiring == HiringStatus.NotHiring)
                {
                    await HandleNotHiring();
                }
                else if (_hiring == HiringStatus.Hiring)
                {
                    await HandleHiring();
                }

                async Task HandleNotHiring()
                {
                    switch (_buildStage)
                    {
                        case 0:
                            _roles = _message.Content;
                            await _message.Author.TrySendMessageAsync("Which specific skills do you have? (*Ie. Unity, C#, Photoshop, Microsoft Excel etc.*)");
                            _buildStage++;
                            break;

                        case 1:
                            _skills = _message.Content;
                            await _message.Author.TrySendMessageAsync("Please list any previous projects or portfolio if you have one. (*N/A if none*)");
                            _buildStage++;
                            break;

                        case 2:
                            _portfolio = _message.Content;
                            await _message.Author.TrySendMessageAsync("How much experience do you have in the field? (*Ie. 2 Months, 5 Years etc.*)");
                            _buildStage++;
                            break;

                        case 3:
                            _experience = _message.Content;
                            await _message.Author.TrySendMessageAsync("Add a decription.");
                            _buildStage++;
                            break;

                        case 4:
                            _description = _message.Content;
                            await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the hobby channel");
                            FinalizeQuestionnaire();
                            await BuildHobbyNotHiringEmbed();
                            break;
                    }
                }
                async Task HandleHiring()
                {
                    switch (_buildStage)
                    {
                        case 0:
                            _projectName = _message.Content;
                            await _message.Author.TrySendMessageAsync("Which roles are you looking for?");
                            _buildStage++;
                            break;

                        case 1:
                            _roles = _message.Content;
                            await _message.Author.TrySendMessageAsync("Please list any previous projects or portfolio if you have one. (*N/A if none*)");
                            _buildStage++;
                            break;

                        case 2:
                            _portfolio = _message.Content;
                            await _message.Author.TrySendMessageAsync("What is the current team size?");
                            _buildStage++;
                            break;

                        case 3:
                            _teamSize = _message.Content;
                            await _message.Author.TrySendMessageAsync("What is the project length? (specify if not strict)");
                            _buildStage++;
                            break;

                        case 4:
                            _projectLength = _message.Content;
                            await _message.Author.TrySendMessageAsync("What specific responsibilities will the person being hired will have ? (*Ie.Implementing physics system, writing character backstories etc.*)");
                            _buildStage++;
                            break;

                        case 5:
                            _responsibilities = _message.Content;
                            await _message.Author.TrySendMessageAsync("Please describe your game.");
                            _buildStage++;
                            break;

                        case 6:
                            _description = _message.Content;
                            await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the hobby channel");
                            FinalizeQuestionnaire();
                            await BuildHobbyHiringEmbed();
                            break;
                    }
                }
            }
            private async Task HandleGametestAnswer()
            {
                switch (_buildStage)
                {
                    case 0:
                        _projectName = _message.Content;
                        await _message.Author.TrySendMessageAsync("Which platform(s) is your game made for? (*Ie. Windows, Android etc.*)");
                        _buildStage++;
                        break;

                    case 1:
                        _platforms = _message.Content;
                        await _message.Author.TrySendMessageAsync("Please describe your game.");
                        _buildStage++;
                        break;

                    case 2:
                        _description = _message.Content;
                        await _message.Author.TrySendMessageAsync("Provide a download link for your game. (optional, reply with \"-\" if you don't have a link)");
                        _buildStage++;
                        break;

                    case 3:
                        _link = _message.Content;
                        await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the gametest channel");
                        FinalizeQuestionnaire();
                        await BuildGametestEmbed();
                        break;
                }
            }
            private async Task HandleMentorAnswer(string uppercaseMessage)
            {
                if (_hiring == HiringStatus.Unknown)
                {
                    if (uppercaseMessage.Contains("TO"))
                    {
                        _hiring = HiringStatus.NotHiring;
                        await _message.Author.TrySendMessageAsync("Selected: **Looking to mentor**.\nOn which subjects are you interested in mentoring?");
                    }
                    else if (uppercaseMessage.Contains("FOR"))
                    {
                        _hiring = HiringStatus.Hiring;
                        await _message.Author.TrySendMessageAsync("Selected: **Looking for a mentor**.\nOn which subjects are you interested in being mentored?");
                    }
                    else
                    {
                        await _message.Author.TrySendMessageAsync("Invalid input.\nAre you looking \n**- For** a mentor\nOr are you looking \n**- To** mentor?");
                    }
                }
                else if (_hiring == HiringStatus.NotHiring)
                {
                    await HandleNotHiring();
                }
                else if (_hiring == HiringStatus.Hiring)
                {
                    await HandleHiring();
                }

                async Task HandleNotHiring()
                {
                    switch (_buildStage)
                    {
                        case 0:
                            _areasOfInterest = _message.Content;
                            await _message.Author.TrySendMessageAsync("(Add a description:");
                            _buildStage++;
                            break;

                        case 1:
                            _description = _message.Content;
                            await _message.Author.TrySendMessageAsync("How much are your rates? (*Ie. \"**Free**\", \"**5€/h**\", \"**10$/h ~ 30$/h**\"*)");
                            _buildStage++;
                            break;

                        case 2:
                            _compensation = _message.Content;
                            await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the mentor channel");
                            FinalizeQuestionnaire();
                            await BuildMentorEmbed();
                            break;
                    }
                }
                async Task HandleHiring()
                {
                    switch (_buildStage)
                    {
                        case 0:
                            _areasOfInterest = _message.Content;
                            await _message.Author.TrySendMessageAsync("Add a description:");
                            _buildStage++;
                            break;

                        case 1:
                            _description = _message.Content;
                            await _message.Author.TrySendMessageAsync("How much are you willing to pay? (*Ie. \"**Free**\", \"**5€/h**\", \"**10$/h ~ 30$/h**\"*)");
                            _buildStage++;
                            break;

                        case 2:
                            _compensation = _message.Content;
                            await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the mentor channel");
                            FinalizeQuestionnaire();
                            await BuildMentorEmbed();
                            break;
                    }
                }
            }

            private void FinalizeQuestionnaire()
            {
                _collab.UpdateCollabTimeout(_message.Author);
                _collab.DeactivateUser(_message.Author);
            }

            public async Task BuildPaidNotHiringEmbed()
            {
                IMessageChannel channel = _client.GetGuild(_data.Configuration.GuildID).GetChannel(_data.Configuration.PaidChannelId) as IMessageChannel;

                await new EmbedBuilder().WithTitle("Looking for Work")
                    .WithDescription(_description)
                    .WithAuthor(_message.Author)
                    .WithColor(Color.Blue)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("My Role", _roles, true)
                    .AddField("My Skills", _skills, true)
                    .AddField("My Portfolio", _portfolio, true)
                    .AddField("Experience in Field", _experience, true)
                    .AddField("My Rates", _compensation, true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build()
                    .SendToChannel(channel);
            }
            public async Task BuildPaidHiringEmbed()
            {
                IMessageChannel channel = _client.GetGuild(_data.Configuration.GuildID).GetChannel(_data.Configuration.PaidChannelId) as IMessageChannel;

                await new EmbedBuilder().WithTitle("Hiring")
                    .WithDescription(_description)
                    .WithAuthor(_message.Author)
                    .WithColor(Color.Green)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("Project Title", _projectName, true)
                    .AddField("Role(s) required", _roles, true)
                    .AddField("Previous Projects/Portfolio", _portfolio, true)
                    .AddField("Current Team Size", _teamSize, true)
                    .AddField("Project Length", _projectLength, true)
                    .AddField("Compensation", _compensation, true)
                    .AddField("Responsibilities", _responsibilities, true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build()
                    .SendToChannel(channel);
            }
            public async Task BuildHobbyNotHiringEmbed()
            {
                IMessageChannel channel = _client.GetGuild(_data.Configuration.GuildID).GetChannel(_data.Configuration.HobbyChannelId) as IMessageChannel;

                await new EmbedBuilder().WithTitle("Looking for work")
                    .WithDescription(_description)
                    .WithAuthor(_message.Author)
                    .WithColor(Color.Blue)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("My Role(s)", _roles, true)
                    .AddField("My Skills", _skills, true)
                    .AddField("Previous Projects/Portfolio", _portfolio, true)
                    .AddField("Experience in the field", _experience, true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build()
                    .SendToChannel(channel);
            }
            public async Task BuildHobbyHiringEmbed()
            {
                IMessageChannel channel = _client.GetGuild(_data.Configuration.GuildID).GetChannel(_data.Configuration.HobbyChannelId) as IMessageChannel;

                await new EmbedBuilder().WithTitle("Hiring")
                    .WithDescription(_description)
                    .WithAuthor(_message.Author)
                    .WithColor(Color.Green)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("Project Title", _projectName, true)
                    .AddField("Role(s) Required", _roles, true)
                    .AddField("Previous Projects/Portfolio", _portfolio, true)
                    .AddField("Current Team Size", _teamSize, true)
                    .AddField("Project Length", _projectLength, true)
                    .AddField("Responsibilities", _responsibilities, true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build()
                    .SendToChannel(channel);
            }
            public async Task BuildGametestEmbed()
            {
                bool hasLink = _link != "-" && _link != "\"-\"";

                IMessageChannel channel = _client.GetGuild(_data.Configuration.GuildID).GetChannel(_data.Configuration.GametestChannelId) as IMessageChannel;

                await new EmbedBuilder().WithTitle(_projectName)
                    .WithDescription(_description)
                    .WithAuthor(_message.Author)
                    .WithColor(Color.Orange)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("Platforms", _platforms, true)
                    .AddFieldConditional(hasLink, "Download Link", _link, true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build()
                    .SendToChannel(channel);
            }
            public async Task BuildMentorEmbed()
            {
                string title = _hiring == HiringStatus.Hiring ? "Looking for a mentor" : "Looking to mentor";
                Color color = _hiring == HiringStatus.Hiring ? Color.Green : Color.Blue;

                IMessageChannel channel = _client.GetGuild(_data.Configuration.GuildID).GetChannel(_data.Configuration.MentorChannelId) as IMessageChannel;                

                await new EmbedBuilder().WithTitle(title)
                    .WithDescription(_description)
                    .WithAuthor(_message.Author)
                    .WithColor(color)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("Areas of Interest", _areasOfInterest, true)
                    .AddField("Rates", _compensation, true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build()
                    .SendToChannel(channel);
            }
        }       
    }
}