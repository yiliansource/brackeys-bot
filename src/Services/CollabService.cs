using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace BrackeysBot.Services
{
    public class CollabService : BrackeysBotService
    {
        private enum CollabChannel
        {
            Paid,
            Hobby,
            Gametest,
            Mentor
        }


        private readonly DataService _data;
        private readonly DiscordSocketClient _client;
        private readonly Dictionary<ulong, int> _lastCollabUsages = new Dictionary<ulong, int>();
        private readonly Dictionary<ulong, Conversation> _activeConversations = new Dictionary<ulong, Conversation>();

        public ulong CollabUserID { get; private set; } //NOT INSTANCED, FIX!!!

        /// <inheritdoc />
        public CollabService(DataService data, DiscordSocketClient client)
        {
            _data = data;
            _client = client;
        }

        public void UpdateCollabTimeout(IUser user)
        {
            _lastCollabUsages.Remove(user.Id);
            _lastCollabUsages.Add(user.Id, Environment.TickCount);
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
                var _conversation = new Conversation(_client, _data, this);
                _activeConversations.Add(user.Id, _conversation);
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
            => _activeConversations.Remove(user.Id);

        public async Task Converse(SocketUserMessage message)
        {
            var userId = message.Author.Id;
            _activeConversations[userId].UpdateMessage(message);
            await _activeConversations[userId].HandleAnswer();
        }

        private class Conversation
        {
            public static int InstanceCount;

            private readonly DiscordSocketClient _client;
            private readonly DataService _data;
            private readonly CollabService _collab;
           
            private SocketUserMessage _message;
            
            private int _buildStage = 0;
            private CollabChannel _collabChannel;
            private bool _hiring;
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
            
            public Conversation(DiscordSocketClient client, DataService data, CollabService collab)
            {
                InstanceCount++;

                _client = client;
                _data = data;
                _collab = collab;
            }

            public void UpdateMessage(SocketUserMessage message)
                => _message = message;

            public async Task HandleAnswer()
            {
                if (_message == null) return;

                var uppercaseMessage = _message.Content.ToUpper();

                switch (_buildStage)
                {
                    case 0:
                        // Channel Select
                        if (uppercaseMessage.Contains("PAID") || _message.Content == "1")
                        {
                            _collabChannel = CollabChannel.Paid;
                            await _message.Author.TrySendMessageAsync("Selected Channel: **Paid**.\nAre you looking for **work** or looking to **hire**?");
                            _buildStage++;
                        }
                        else if (uppercaseMessage.Contains("HOBBY") || _message.Content == "2")
                        {
                            _collabChannel = CollabChannel.Hobby;
                            await _message.Author.TrySendMessageAsync("Selected Channel: **Hobby**.\nAre you looking for a **team** or looking for **people**?");
                            _buildStage++;
                        }
                        else if (uppercaseMessage.Contains("GAMETEST") || _message.Content == "3")
                        {
                            _collabChannel = CollabChannel.Gametest;
                            await _message.Author.TrySendMessageAsync("Selected Channel: **Gametest**.\nWhat is the name of your project?");
                            _buildStage++;
                        }
                        else if (uppercaseMessage.Contains("MENTOR") || _message.Content == "4")
                        {
                            _collabChannel = CollabChannel.Mentor;
                            await _message.Author.TrySendMessageAsync("Selected Channel: **Mentor**.\nAre you looking **for** a mentor or looking **to** mentor people?"); // Too ambiguous?
                            _buildStage++;
                        }
                        else
                        {
                            await _message.Author.TrySendMessageAsync("**Invalid answer!**\nPlease enter which channel you would like to post:\n1- Paid\n2- Hobby\n3- Gametest\n4- Mentor");
                        }
                        break;

                    case 1:
                        // Paid, Not Hiring
                        if (_collabChannel == CollabChannel.Paid && uppercaseMessage.Contains("WORK"))
                        {
                            _hiring = false;
                            await _message.Author.TrySendMessageAsync("Selected: **Looking for work**.\nWhat is/are your role(s)?");
                            _buildStage++;
                        }
                        // Paid, Hiring
                        else if (_collabChannel == CollabChannel.Paid && uppercaseMessage.Contains("HIRE"))
                        {
                            _hiring = true;
                            await _message.Author.TrySendMessageAsync("Selected: **Looking to hire**.\nWhat is the name of your project?");
                            _buildStage++;
                        }
                        // Paid, Error
                        else if (_collabChannel == CollabChannel.Paid)
                        {
                            await _message.Author.TrySendMessageAsync("Invalid input.\nAre you looking for\n**- Work**\nOr are you looking to\n**- Hire**?");
                        }
                        
                        // Hobby, Not Hiring
                        else if (_collabChannel == CollabChannel.Hobby && uppercaseMessage.Contains("TEAM"))
                        {
                            _hiring = false;
                            await _message.Author.TrySendMessageAsync("Selected: **Looking for a team**.\nWhat is/are your role(s)?");
                            _buildStage++;
                        }
                        // Hobby, Hiring
                        else if (_collabChannel == CollabChannel.Hobby && uppercaseMessage.Contains("PEOPLE"))
                        {
                            _hiring = true;
                            await _message.Author.TrySendMessageAsync("Selected: **Looking for people**.\nWhat is the name of your project?");
                            _buildStage++;
                        }
                        // Hobby, Error
                        else if (_collabChannel == CollabChannel.Hobby)
                        {
                            await _message.Author.TrySendMessageAsync("Invalid input.\nAre you looking for a \n**- Team**\nOr are you looking for\n**- People**?");
                        }

                        // Gametest
                        else if (_collabChannel == CollabChannel.Gametest)
                        {
                            _projectName = _message.Content;
                            await _message.Author.TrySendMessageAsync("Which platform(s) is your game made for? (*Ie. Windows, Android etc.*)");
                            _buildStage++;
                        }

                        // Mentor, Hiring
                        else if (_collabChannel == CollabChannel.Mentor && uppercaseMessage.Contains("FOR"))
                        {
                            _hiring = true;
                            await _message.Author.TrySendMessageAsync("Selected: **Looking for a mentor**.\nOn which subjects are you interested in being mentored?");
                            _buildStage++;
                        }
                        // Mentor, Not Hiring
                        else if (_collabChannel == CollabChannel.Mentor && uppercaseMessage.Contains("TO"))
                        {
                            _hiring = false;
                            await _message.Author.TrySendMessageAsync("Selected: **Looking to mentor**.\nOn which subjects are you interested in mentoring?");
                            _buildStage++;
                        }
                        // Mentor, Error
                        else if (_collabChannel == CollabChannel.Mentor)
                        {
                            await _message.Author.TrySendMessageAsync("Invalid input.\nAre you looking \n**- For** a mentor\nOr are you looking \n**- To** mentor?");
                        }
                        break;

                    case 2:
                        // Paid, Not Hiring
                        if (_collabChannel == CollabChannel.Paid && !_hiring)
                        {
                            _roles = _message.Content;
                            await _message.Author.TrySendMessageAsync("Which specific skills do you have? (*Ie. Unity, C#, Photoshop, Microsoft Excel etc.*)");
                            _buildStage++;
                        }
                        // Paid, Hiring
                        else if (_collabChannel == CollabChannel.Paid && _hiring)
                        {
                            _projectName = _message.Content;
                            await _message.Author.TrySendMessageAsync("Describe your project.");
                            _buildStage++;
                        }
                       

                        // Hobby, Not Hiring
                        else if (_collabChannel == CollabChannel.Hobby && !_hiring)
                        {
                            _roles = _message.Content;
                            await _message.Author.TrySendMessageAsync("Which specific skills do you have? (*Ie. Unity, C#, Photoshop, Microsoft Excel etc.*)");
                            _buildStage++;
                        }
                        // Hobby, Hiring
                        else if (_collabChannel == CollabChannel.Hobby && _hiring)
                        {
                            _projectName = _message.Content;
                            await _message.Author.TrySendMessageAsync("Which roles are you looking for?");
                            _buildStage++;
                        }

                        // Gametest
                        else if (_collabChannel == CollabChannel.Gametest)
                        {
                            _platforms = _message.Content;
                            await _message.Author.TrySendMessageAsync("Please describe your game.");
                            _buildStage++;
                        }

                        // Mentor, Not Hiring
                        else if (_collabChannel == CollabChannel.Mentor && !_hiring)
                        {
                            _areasOfInterest = _message.Content;
                            await _message.Author.TrySendMessageAsync("(*Optional*) Add a description:");
                            _buildStage++;
                        }
                        // Mentor Hiring
                        else if (_collabChannel == CollabChannel.Mentor && _hiring)
                        {
                            _areasOfInterest = _message.Content;
                            await _message.Author.TrySendMessageAsync("(*Optional*) Add a description:");
                            _buildStage++;
                        }
                        break;

                    case 3:
                        // Paid, Not Hiring
                        if (_collabChannel == CollabChannel.Paid && !_hiring)
                        {
                            _skills = _message.Content;
                            await _message.Author.TrySendMessageAsync("Please list any previous projects or portfolio if you have one. (*N/A if none*)");
                            _buildStage++;
                        }
                        // Paid, Hiring                      
                        else if (_collabChannel == CollabChannel.Paid && _hiring)
                        {
                            _description = _message.Content;
                            await _message.Author.TrySendMessageAsync("Which roles are you looking to hire?");
                            _buildStage++;
                        }

                        // Hobby, Not Hiring
                        else if (_collabChannel == CollabChannel.Hobby && !_hiring)
                        {
                            _skills = _message.Content;
                            await _message.Author.TrySendMessageAsync("Please list any previous projects or portfolio if you have one. (*N/A if none*)");
                            _buildStage++;
                        }
                        // Hobby, Hiring
                        else if (_collabChannel == CollabChannel.Hobby && _hiring)
                        {
                            _roles = _message.Content;
                            await _message.Author.TrySendMessageAsync("Please list any previous projects or portfolio if you have one. (*N/A if none*)");
                            _buildStage++;
                        }

                        // Gametest
                        else if (_collabChannel == CollabChannel.Gametest)
                        {
                            _description = _message.Content;
                            await _message.Author.TrySendMessageAsync("Provide a download link for your game. (optional, reply with \"-\" if you don't have a link)");
                            FinalizeQuestionnaire();
                            await BuildGametestEmbed();
                        }

                        // Mentor, Not Hiring
                        else if (_collabChannel == CollabChannel.Mentor && !_hiring)
                        {
                            _description = _message.Content;
                            await _message.Author.TrySendMessageAsync("How much are your rates? (*Ie. \"**Free**\", \"**5€/h**\", \"**10$/h ~ 30$/h**\"*)");
                            _buildStage++;
                        }
                        // Mentor Hiring
                        else if (_collabChannel == CollabChannel.Mentor && _hiring)
                        {
                            _description = _message.Content;
                            await _message.Author.TrySendMessageAsync("How much are you willing to pay? (*Ie. \"**Free**\", \"**5€/h**\", \"**10$/h ~ 30$/h**\"*)");
                            _buildStage++;
                        }
                        break;

                    case 4:
                        // Paid, Not Hiring
                        if (_collabChannel == CollabChannel.Paid && !_hiring)
                        {
                            _portfolio = _message.Content;
                            await _message.Author.TrySendMessageAsync("How much experience do you have in the field? (*Ie. 2 Months, 5 Years etc.*)");
                            _buildStage++;
                        }
                        // Paid, Hiring
                        else if (_collabChannel == CollabChannel.Paid && _hiring)
                        {
                            _roles = _message.Content;
                            await _message.Author.TrySendMessageAsync("Please list any previous projects or portfolio if you have one. (**N/A** *if none*)");
                            _buildStage++;
                        }
                        

                        // Hobby, Not Hiring
                        else if (_collabChannel == CollabChannel.Hobby && !_hiring)
                        {
                            _portfolio = _message.Content;
                            await _message.Author.TrySendMessageAsync("How much experience do you have in the field? (*Ie. 2 Months, 5 Years etc.*)");
                            _buildStage++;
                        }
                        // Hobby, Hiring
                        else if (_collabChannel == CollabChannel.Hobby && _hiring)
                        {
                            _portfolio = _message.Content;
                            await _message.Author.TrySendMessageAsync("What is the current team size?");
                            _buildStage++;
                        }

                        // Gametest
                        else if (_collabChannel == CollabChannel.Gametest)
                        {
                            _link = _message.Content;
                            await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the gametest channel");
                            FinalizeQuestionnaire();
                            await BuildGametestEmbed();
                        }

                        // Mentor
                        else if (_collabChannel == CollabChannel.Mentor)
                        {
                            _compensation = _message.Content;
                            await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the mentor channel");
                            FinalizeQuestionnaire();
                            await BuildMentorEmbed();
                        }
                        break;

                    case 5:

                        // Paid, Not Hiring
                        if (_collabChannel == CollabChannel.Paid && !_hiring)
                        {
                            _experience = _message.Content;
                            await _message.Author.TrySendMessageAsync("Add a description. (Optional)");
                            _buildStage++;
                        }
                        // Paid, Hiring
                        else if (_collabChannel == CollabChannel.Paid && _hiring)
                        {
                            _portfolio = _message.Content;
                            await _message.Author.TrySendMessageAsync("What is the current team size?");
                            _buildStage++;
                        }

                        // Hobby, Not Hiring
                        else if (_collabChannel == CollabChannel.Hobby && !_hiring)
                        {
                            _experience = _message.Content;
                            await _message.Author.TrySendMessageAsync("Add a decription.");
                            _buildStage++;
                        }
                        // Hobby, Hiring
                        else if (_collabChannel == CollabChannel.Hobby && _hiring)
                        {
                            _teamSize = _message.Content;
                            await _message.Author.TrySendMessageAsync("What is the project length? (specify if not strict)");
                            _buildStage++;
                        }
                        break;

                    case 6:
                        // Paid, Not Hiring
                        if (_collabChannel == CollabChannel.Paid && !_hiring)
                        {
                            _description = _message.Content;
                            await _message.Author.TrySendMessageAsync("How much are your rates ? (*Ie. \"**5$/work done**\", \"**5€/h**\", \"**10$/h ~ 30$/h**\"*)");
                            _buildStage++;
                        }
                        // Paid, Hiring
                        else if (_collabChannel == CollabChannel.Paid && _hiring)
                        {
                            _teamSize = _message.Content;
                            await _message.Author.TrySendMessageAsync("What is the project length? (specify if not strict)");
                            _buildStage++;
                        }
                       
                        // Hobby, Not Hiring
                        else if (_collabChannel == CollabChannel.Hobby && !_hiring)
                        {
                            _description = _message.Content;
                            await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the hobby channel");
                            FinalizeQuestionnaire();
                            await BuildHobbyNotHiringEmbed();
                        }
                        // Hobby, Hiring
                        else if (_collabChannel == CollabChannel.Hobby && _hiring)
                        {
                            _projectLength = _message.Content;
                            await _message.Author.TrySendMessageAsync("What specific responsibilities will the person being hired will have ? (*Ie.Implementing physics system, writing character backstories etc.*)");
                            _buildStage++;
                        }
                        break;

                    case 7:
                        // Paid, Not Hiring
                        if (_collabChannel == CollabChannel.Paid && !_hiring)
                        {
                            _compensation = _message.Content;
                            await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the paid channel");
                            FinalizeQuestionnaire();
                            await BuildPaidNotHiringEmbed();
                        }
                        // Paid, Hiring
                        else if (_collabChannel == CollabChannel.Paid && _hiring)
                        {
                            _projectLength = _message.Content;
                            await _message.Author.TrySendMessageAsync("What is the compensation? (*Ie. \"**5$/work done**\", \"**5€/h**\", \"**10$/h ~ 30$/h**\"*)");
                            _buildStage++;
                        }

                        // Hobby, Hiring
                        else if (_collabChannel == CollabChannel.Hobby && _hiring)
                        {
                            _responsibilities = _message.Content;
                            await _message.Author.TrySendMessageAsync("Please describe your game.");
                            _buildStage++;
                        }
                        break;

                    case 8:
                        // Paid, Hiring
                        if (_collabChannel == CollabChannel.Paid && _hiring)
                        {
                            _compensation = _message.Content;
                            await _message.Author.TrySendMessageAsync("What specific responsibilities will the person being hired will have? (*Ie. Implementing physics system, writing character backstories etc.*)");
                            _buildStage++;
                        }

                        // Hobby, Hiring
                        else if (_collabChannel == CollabChannel.Hobby && _hiring)
                        {
                            _description = _message.Content;
                            await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the hobby channel");
                            FinalizeQuestionnaire();
                            await BuildHobbyHiringEmbed();
                        }
                        break;

                    case 9:
                        // Paid, Hiring
                        if (_collabChannel == CollabChannel.Paid && _hiring)
                        {
                            _responsibilities = _message.Content;
                            await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the paid channel");
                            FinalizeQuestionnaire();
                            await BuildPaidHiringEmbed();
                        }

                        break;

                    default:
                        break;
                }
            }

            private void FinalizeQuestionnaire()
            {
                _collab.UpdateCollabTimeout(_message.Author);
                _collab.DeactivateUser(_message.Author);
            }

            public async Task BuildPaidNotHiringEmbed()   // Test
            {
                var title = "Looking for Work";
                var color = Color.Green;

                var channel = _client.GetGuild(_data.Configuration.GuildID).GetChannel(_data.Configuration.PaidChannelId) as IMessageChannel;

                await new EmbedBuilder().WithTitle(title)
                    .WithDescription(_description)
                    .WithAuthor(_message.Author)
                    .WithColor(color)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("My Role", _roles, true)
                    .AddField("My Skills", _skills, true)
                    .AddField("My Portfolio", _portfolio, true)
                    .AddField("Experience in Field", _experience, true)
                    .AddField("My Rates", _compensation, true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build()
                    .SendToChannel(channel);

                Console.WriteLine("InstanceCount: " + InstanceCount);
            }
            public async Task BuildPaidHiringEmbed()      // Test
            {
                var title = "Hiring";
                var color = Color.Blue;

                var channel = _client.GetGuild(_data.Configuration.GuildID).GetChannel(_data.Configuration.PaidChannelId) as IMessageChannel;

                await new EmbedBuilder().WithTitle(title)
                    .WithDescription(_description)
                    .WithAuthor(_message.Author)
                    .WithColor(color)
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

                Console.WriteLine("InstanceCount: " + InstanceCount);
            }
            public async Task BuildHobbyNotHiringEmbed()  // Test
            {
                var title = "Looking for work";
                var color = Color.Green;

                var channel = _client.GetGuild(_data.Configuration.GuildID).GetChannel(_data.Configuration.HobbyChannelId) as IMessageChannel;

                await new EmbedBuilder().WithTitle(title)
                    .WithDescription(_description)
                    .WithAuthor(_message.Author)
                    .WithColor(color)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("My Role(s)", _roles, true)
                    .AddField("My Skills", _skills, true)
                    .AddField("Previous Projects/Portfolio", _portfolio, true)
                    .AddField("Experience in the field", _experience, true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build()
                    .SendToChannel(channel);

                Console.WriteLine("InstanceCount: " + InstanceCount);
            }
            public async Task BuildHobbyHiringEmbed()     // Test
            {
                var title = "Hiring";
                var color = Color.Blue;

                var channel = _client.GetGuild(_data.Configuration.GuildID).GetChannel(_data.Configuration.HobbyChannelId) as IMessageChannel;

                await new EmbedBuilder().WithTitle(title)
                    .WithDescription(_description)
                    .WithAuthor(_message.Author)
                    .WithColor(color)
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

                Console.WriteLine("InstanceCount: " + InstanceCount);
            }
            public async Task BuildGametestEmbed()        // Test
            {
                var hasLink = _link != "-" || _link != "\"-\"";

                var channel = _client.GetGuild(_data.Configuration.GuildID).GetChannel(_data.Configuration.GametestChannelId) as IMessageChannel;

                await new EmbedBuilder().WithTitle(_projectName)
                    .WithDescription(_description)
                    .WithAuthor(_message.Author)
                    .WithColor(Color.Blue)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("Platforms", _platforms, true)
                    .AddField("Project Name", _projectName, true)
                    .AddFieldConditional(hasLink, "Download Link", _link, true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build()
                    .SendToChannel(channel);
            }
            public async Task BuildMentorEmbed()
            {
                var title = _hiring ? "Looking for a mentor" : "Looking to mentor";
                var color = _hiring ? Color.Blue : Color.Green;

                var channel = _client.GetGuild(_data.Configuration.GuildID).GetChannel(_data.Configuration.MentorChannelId) as IMessageChannel;                

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

                Console.WriteLine("InstanceCount: " + InstanceCount);
            }

            ~Conversation()
            {
                InstanceCount--;
            }
        }       
    }
}
