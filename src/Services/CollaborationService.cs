using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace BrackeysBot.Services
{
    public class CollaborationService : BrackeysBotService
    {      
        private readonly DataService _data;
        private readonly DiscordSocketClient _client;
        private readonly FilterService _filterService;
        private readonly ConcurrentDictionary<ulong, int> _lastCollabUsages = new ConcurrentDictionary<ulong, int>();
        private readonly ConcurrentDictionary<ulong, CollabConversation> _activeConversations = new ConcurrentDictionary<ulong, CollabConversation>();

        /// <inheritdoc />
        public CollaborationService(DataService data, DiscordSocketClient client, FilterService filterService)
        {
            _data = data;
            _client = client;
            _filterService = filterService;
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
            if (_filterService.ContainsBlockedWord(message.Content))
            {
                await message.Author.TrySendMessageAsync("Seems like you were using a blocked word! Please try again, but without rude words and racial slurs.");
                return;
            }
        
            ulong userId = message.Author.Id;
            _activeConversations[userId].UpdateMessage(message);
            await _activeConversations[userId].HandleAnswer();
        }

        private class CollabConversation
        {
            private readonly DiscordSocketClient _client;
            private readonly DataService _data;
            private readonly CollaborationService _collab;
            private readonly SocketGuild _guild;

            // Characters which need escaping
            private static readonly string[] SensitiveCharacters = { "\\", "*", "_", "~", "`", "|", ">", "[", "(" };

            public CollabConversation(DiscordSocketClient client, DataService data, CollaborationService collab)
            {
                _client = client;
                _data = data;
                _collab = collab;
                _guild =_client.GetGuild(_data.Configuration.GuildID);
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
            private Embed _embed;

            private int _buildStage = 0;
            private CollabChannel _collabChannel = CollabChannel.Unknown;
            private HiringStatus _hiring = HiringStatus.Unknown;

            private ConcurrentDictionary<string, string> _fields = new ConcurrentDictionary<string, string>();

            public void UpdateMessage(SocketUserMessage message)
                => _message = message;
            public async Task HandleAnswer()
            {
                if (_message == null)
                    return;

                string message = _message.Content;

                // Channel Select
                if (_collabChannel == CollabChannel.Unknown)
                {
                    if (message.Contains("PAID", StringComparison.OrdinalIgnoreCase) || message == "1")
                    {
                        _collabChannel = CollabChannel.Paid;
                        await _message.Author.TrySendMessageAsync("Selected Channel: **Paid**.\nAre you looking for **work** or looking to **hire**?");
                    }
                    else if (message.Contains("HOBBY", StringComparison.OrdinalIgnoreCase) || message == "2")
                    {
                        _collabChannel = CollabChannel.Hobby;
                        await _message.Author.TrySendMessageAsync("Selected Channel: **Hobby**.\nAre you looking for a **team** or looking for **people**?");
                    }
                    else if (message.Contains("GAMETEST", StringComparison.OrdinalIgnoreCase) || message == "3")
                    {
                        _collabChannel = CollabChannel.Gametest;
                        await _message.Author.TrySendMessageAsync("Selected Channel: **Gametest**.\nWhat is the name of your project?");
                    }
                    else if (message.Contains("MENTOR", StringComparison.OrdinalIgnoreCase) || message == "4")
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
                    await HandlePaidAnswer(message);
                }
                else if (_collabChannel == CollabChannel.Hobby)
                {
                    await HandleHobbyAnswer(message);
                }
                else if (_collabChannel == CollabChannel.Gametest)
                {
                    await HandleGametestAnswer();
                }
                else if (_collabChannel == CollabChannel.Mentor)
                {
                    await HandleMentorAnswer(message);
                }
            }

            private async Task HandlePaidAnswer(string message)
            {
                if (_hiring == HiringStatus.Unknown)
                {
                    if (message.Contains("WORK", StringComparison.OrdinalIgnoreCase))
                    {
                        _hiring = HiringStatus.NotHiring;
                        await _message.Author.TrySendMessageAsync("Selected: **Looking for work**.\nWhat is/are your role(s)?");
                    }
                    else if (message.Contains("HIRE", StringComparison.OrdinalIgnoreCase))
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
                            _fields.TryAdd("roles", _message.Content);
                            await _message.Author.TrySendMessageAsync("Which specific skills do you have? (*Ie. Unity, C#, Photoshop, Microsoft Excel etc.*)");
                            _buildStage++;
                            break;

                        case 1:
                            _fields.TryAdd("skills", _message.Content);
                            await _message.Author.TrySendMessageAsync("Please list any previous projects or portfolio if you have one. (*N/A if none*)");
                            _buildStage++;
                            break;

                        case 2:
                            _fields.TryAdd("portfolio", _message.Content);
                            await _message.Author.TrySendMessageAsync("How much experience do you have in the field? (*Ie. 2 Months, 5 Years etc.*)");
                            _buildStage++;
                            break;

                        case 3:
                            _fields.TryAdd("experience", _message.Content);
                            await _message.Author.TrySendMessageAsync("Add a description. (Optional)");
                            _buildStage++;
                            break;

                        case 4:
                            _fields.TryAdd("description", _message.Content);
                            await _message.Author.TrySendMessageAsync("How much are your rates ? (*Ie. \"**5$/work done**\", \"**5€/h**\", \"**10$/h ~ 30$/h**\"*)");
                            _buildStage++;
                            break;

                        case 5:
                            _fields.TryAdd("compensation", _message.Content);
                            foreach (var pair in _fields)
                            {
                                _fields[pair.Key] = SanitizeMarkdown(pair);
                            }
                            _embed = BuildPaidNotHiringEmbed();
                            await _message.Author.TrySendMessageAsync("This will be sent to the paid channel. Reply with \"**Yes**\" to approve, or \"**No**\" to restart the process.", embed: _embed);
                            _buildStage++;
                            break;

                        case 6:
                            if (_message.Content.Equals("YES", StringComparison.OrdinalIgnoreCase))
                            {
                                IMessageChannel channel = _guild.GetChannel(_data.Configuration.PaidChannelId) as IMessageChannel;

                                FinalizeQuestionnaire();

                                await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the paid channel");
                                await channel.SendMessageAsync(text: $"Submitted by {_message.Author.Mention}:", embed: _embed);
                            }
                            else if (_message.Content.Equals("NO", StringComparison.OrdinalIgnoreCase))
                            {
                                _collabChannel = CollabChannel.Unknown;
                                _hiring = HiringStatus.Unknown;
                                _fields = new ConcurrentDictionary<string, string>();
                                _buildStage = 0;

                                await _message.Author.TrySendMessageAsync($"**Process restarted!**\nPlease enter which channel you would like to post:\n1- Paid\n2- Hobby\n3- Gametest\n4- Mentor");
                            }
                            else
                            {
                                await _message.Author.TrySendMessageAsync("Input invalid. Reply with \"**Yes**\" to approve, or \"**No**\" to restart the process.");
                            }
                            break;
                    }
                }
                async Task HandleHiring()
                {
                    switch (_buildStage)
                    {
                        case 0:
                            _fields.TryAdd("projectName", _message.Content);
                            await _message.Author.TrySendMessageAsync("Describe your project.");
                            _buildStage++;
                            break;

                        case 1:
                            _fields.TryAdd("description", _message.Content);
                            await _message.Author.TrySendMessageAsync("Which roles are you looking to hire?");
                            _buildStage++;
                            break;

                        case 2:
                            _fields.TryAdd("roles", _message.Content);
                            await _message.Author.TrySendMessageAsync("Please list any previous projects or portfolio if you have one. (**N/A** *if none*)");
                            _buildStage++;
                            break;

                        case 3:
                            _fields.TryAdd("portfolio", _message.Content);
                            await _message.Author.TrySendMessageAsync("What is the current team size?");
                            _buildStage++;
                            break;

                        case 4:
                            _fields.TryAdd("teamSize", _message.Content);
                            await _message.Author.TrySendMessageAsync("What is the project length? (specify if not strict)");
                            _buildStage++;
                            break;

                        case 5:
                            _fields.TryAdd("projectLength", _message.Content);
                            await _message.Author.TrySendMessageAsync("What is the compensation? (*Ie. \"**5$/work done**\", \"**5€/h**\", \"**10$/h ~ 30$/h**\"*)");
                            _buildStage++;
                            break;

                        case 6:
                            _fields.TryAdd("compensation", _message.Content);
                            await _message.Author.TrySendMessageAsync("What specific responsibilities will the person being hired will have? (*Ie. Implementing physics system, writing character backstories etc.*)");
                            _buildStage++;
                            break;

                        case 7:
                            _fields.TryAdd("responsibilities", _message.Content);
                            foreach (var pair in _fields)
                            {
                                _fields[pair.Key] = SanitizeMarkdown(pair);
                            }
                            _embed = BuildPaidHiringEmbed();
                            await _message.Author.TrySendMessageAsync("This will be sent to the paid channel. Reply with \"**Yes**\" to approve, or \"**No**\" to restart the process.", embed: _embed);
                            _buildStage++;
                            break;

                        case 8:
                            if (_message.Content.Equals("YES", StringComparison.OrdinalIgnoreCase))
                            {
                                IMessageChannel channel = _guild.GetChannel(_data.Configuration.PaidChannelId) as IMessageChannel;

                                FinalizeQuestionnaire();

                                await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the paid channel");
                                await channel.SendMessageAsync(text: $"Submitted by {_message.Author.Mention}:", embed: _embed);
                            }
                            else if (_message.Content.Equals("NO", StringComparison.OrdinalIgnoreCase))
                            {
                                _collabChannel = CollabChannel.Unknown;
                                _hiring = HiringStatus.Unknown;
                                _fields = new ConcurrentDictionary<string, string>();
                                _buildStage = 0;

                                await _message.Author.TrySendMessageAsync($"**Process restarted!**\nPlease enter which channel you would like to post:\n1- Paid\n2- Hobby\n3- Gametest\n4- Mentor");
                            }
                            else
                            {
                                await _message.Author.TrySendMessageAsync("Input invalid. Reply with \"**Yes**\" to approve, or \"**No**\" to restart the process.");
                            }
                            break;
                    }
                }
            }              
            private async Task HandleHobbyAnswer(string message)
            {
                if (_hiring == HiringStatus.Unknown)
                {
                    if (message.Contains("TEAM", StringComparison.OrdinalIgnoreCase))
                    {
                        _hiring = HiringStatus.NotHiring;
                        await _message.Author.TrySendMessageAsync("Selected: **Looking for a team**.\nWhat is/are your role(s)?");
                    }
                    else if (message.Contains("PEOPLE", StringComparison.OrdinalIgnoreCase))
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
                            _fields.TryAdd("roles", _message.Content);
                            await _message.Author.TrySendMessageAsync("Which specific skills do you have? (*Ie. Unity, C#, Photoshop, Microsoft Excel etc.*)");
                            _buildStage++;
                            break;

                        case 1:
                            _fields.TryAdd("skills", _message.Content);
                            await _message.Author.TrySendMessageAsync("Please list any previous projects or portfolio if you have one. (*N/A if none*)");
                            _buildStage++;
                            break;

                        case 2:
                            _fields.TryAdd("portfolio", _message.Content);
                            await _message.Author.TrySendMessageAsync("How much experience do you have in the field? (*Ie. 2 Months, 5 Years etc.*)");
                            _buildStage++;
                            break;

                        case 3:
                            _fields.TryAdd("experience", _message.Content);
                            await _message.Author.TrySendMessageAsync("Add a decription.");
                            _buildStage++;
                            break;

                        case 4:
                            _fields.TryAdd("description", _message.Content);
                            foreach (var pair in _fields)
                            {
                                _fields[pair.Key] = SanitizeMarkdown(pair);
                            }
                            _embed = BuildHobbyNotHiringEmbed();
                            await _message.Author.TrySendMessageAsync("This will be sent to the hobby channel. Reply with \"**Yes**\" to approve, or \"**No**\" to restart the process.", embed: _embed);
                            _buildStage++;
                            break;

                        case 5:
                            if (_message.Content.Equals("YES", StringComparison.OrdinalIgnoreCase))
                            {
                                IMessageChannel channel = _guild.GetChannel(_data.Configuration.HobbyChannelId) as IMessageChannel;

                                FinalizeQuestionnaire();

                                await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the hobby channel");
                                await channel.SendMessageAsync(text: $"Submitted by {_message.Author.Mention}:", embed: _embed);
                            }
                            else if (_message.Content.Equals("NO", StringComparison.OrdinalIgnoreCase))
                            {
                                _collabChannel = CollabChannel.Unknown;
                                _hiring = HiringStatus.Unknown;
                                _fields = new ConcurrentDictionary<string, string>();
                                _buildStage = 0;

                                await _message.Author.TrySendMessageAsync($"**Process restarted!**\nPlease enter which channel you would like to post:\n1- Paid\n2- Hobby\n3- Gametest\n4- Mentor");
                            }
                            else
                            {
                                await _message.Author.TrySendMessageAsync("Input invalid. Reply with \"**Yes**\" to approve, or \"**No**\" to restart the process.");
                            }
                            break;
                    }
                }
                async Task HandleHiring()
                {
                    switch (_buildStage)
                    {
                        case 0:
                            _fields.TryAdd("projectName", _message.Content);
                            await _message.Author.TrySendMessageAsync("Which roles are you looking for?");
                            _buildStage++;
                            break;

                        case 1:
                            _fields.TryAdd("roles", _message.Content);
                            await _message.Author.TrySendMessageAsync("Please list any previous projects or portfolio if you have one. (*N/A if none*)");
                            _buildStage++;
                            break;

                        case 2:
                            _fields.TryAdd("portfolio", _message.Content);
                            await _message.Author.TrySendMessageAsync("What is the current team size?");
                            _buildStage++;
                            break;

                        case 3:
                            _fields.TryAdd("teamSize", _message.Content);
                            await _message.Author.TrySendMessageAsync("What is the project length? (specify if not strict)");
                            _buildStage++;
                            break;

                        case 4:
                            _fields.TryAdd("projectLength", _message.Content);
                            await _message.Author.TrySendMessageAsync("What specific responsibilities will the person being hired will have ? (*Ie.Implementing physics system, writing character backstories etc.*)");
                            _buildStage++;
                            break;

                        case 5:
                            _fields.TryAdd("responsibilities", _message.Content);
                            await _message.Author.TrySendMessageAsync("Please describe your game.");
                            _buildStage++;
                            break;

                        case 6:
                            _fields.TryAdd("description", _message.Content);
                            foreach (var pair in _fields)
                            {
                                _fields[pair.Key] = SanitizeMarkdown(pair);
                            }
                            _embed = BuildHobbyHiringEmbed();
                            await _message.Author.TrySendMessageAsync("This will be sent to the hobby channel. Reply with \"**Yes**\" to approve, or \"**No**\" to restart the process.", embed: _embed);
                            _buildStage++;
                            break;

                        case 7:
                            if (_message.Content.Equals("YES", StringComparison.OrdinalIgnoreCase))
                            {
                                IMessageChannel channel = _guild.GetChannel(_data.Configuration.HobbyChannelId) as IMessageChannel;

                                FinalizeQuestionnaire();

                                await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the hobby channel");
                                await channel.SendMessageAsync(text: $"Submitted by {_message.Author.Mention}:", embed: _embed);
                            }
                            else if (_message.Content.Equals("NO", StringComparison.OrdinalIgnoreCase))
                            {
                                _collabChannel = CollabChannel.Unknown;
                                _hiring = HiringStatus.Unknown;
                                _fields = new ConcurrentDictionary<string, string>();
                                _buildStage = 0;

                                await _message.Author.TrySendMessageAsync($"**Process restarted!**\nPlease enter which channel you would like to post:\n1- Paid\n2- Hobby\n3- Gametest\n4- Mentor");
                            }
                            else
                            {
                                await _message.Author.TrySendMessageAsync("Input invalid. Reply with \"**Yes**\" to approve, or \"**No**\" to restart the process.");
                            }
                            break;
                    }
                }
            }
            private async Task HandleGametestAnswer()
            {
                switch (_buildStage)
                {
                    case 0:
                        _fields.TryAdd("projectName", _message.Content);
                        await _message.Author.TrySendMessageAsync("Which platform(s) is your game made for? (*Ie. Windows, Android etc.*)");
                        _buildStage++;
                        break;

                    case 1:
                        _fields.TryAdd("platforms", _message.Content);
                        await _message.Author.TrySendMessageAsync("Please describe your game.");
                        _buildStage++;
                        break;

                    case 2:
                        _fields.TryAdd("description", _message.Content);
                        await _message.Author.TrySendMessageAsync("Provide a download link for your game. (optional, reply with \"-\" if you don't have a link)");
                        _buildStage++;
                        break;

                    case 3:
                        _fields.TryAdd("link", _message.Content);
                        foreach (var pair in _fields)
                        {
                            _fields[pair.Key] = SanitizeMarkdown(pair);
                        }
                        _embed = BuildGametestEmbed();
                        await _message.Author.TrySendMessageAsync("This will be sent to the gametest channel. Reply with \"**Yes**\" to approve, or \"**No**\" to restart the process.", embed: _embed);
                        _buildStage++;
                        break;

                    case 4:
                        if (_message.Content.Equals("YES", StringComparison.OrdinalIgnoreCase))
                        {
                            IMessageChannel channel = _guild.GetChannel(_data.Configuration.GametestChannelId) as IMessageChannel;

                            FinalizeQuestionnaire();

                            await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the gametest channel");
                            await channel.SendMessageAsync(text: $"Submitted by {_message.Author.Mention}:", embed: _embed);
                        }
                        else if (_message.Content.Equals("NO", StringComparison.OrdinalIgnoreCase))
                        {
                            _collabChannel = CollabChannel.Unknown;
                            _hiring = HiringStatus.Unknown;
                            _fields = new ConcurrentDictionary<string, string>();
                            _buildStage = 0;

                            await _message.Author.TrySendMessageAsync($"**Process restarted!**\nPlease enter which channel you would like to post:\n1- Paid\n2- Hobby\n3- Gametest\n4- Mentor");
                        }
                        else
                        {
                            await _message.Author.TrySendMessageAsync("Input invalid. Reply with \"**Yes**\" to approve, or \"**No**\" to restart the process.");
                        }
                        break;
                }
            }
            private async Task HandleMentorAnswer(string message)
            {
                if (_hiring == HiringStatus.Unknown)
                {
                    if (message.Contains("TO", StringComparison.OrdinalIgnoreCase))
                    {
                        _hiring = HiringStatus.NotHiring;
                        await _message.Author.TrySendMessageAsync("Selected: **Looking to mentor**.\nOn which subjects are you interested in mentoring?");
                    }
                    else if (message.Contains("FOR", StringComparison.OrdinalIgnoreCase))
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
                            _fields.TryAdd("areasOfInterest", _message.Content);
                            await _message.Author.TrySendMessageAsync("Add a description:");
                            _buildStage++;
                            break;

                        case 1:
                            _fields.TryAdd("description", _message.Content);
                            await _message.Author.TrySendMessageAsync("How much are your rates? (*Ie. \"**Free**\", \"**5€/h**\", \"**10$/h ~ 30$/h**\"*)");
                            _buildStage++;
                            break;

                        case 2:
                            _fields.TryAdd("compensation", _message.Content);
                            foreach (var pair in _fields)
                            {
                                _fields[pair.Key] = SanitizeMarkdown(pair);
                            }
                            _embed = BuildMentorEmbed();
                            await _message.Author.TrySendMessageAsync("This will be sent to the mentor channel. Reply with \"**Yes**\" to approve, or \"**No**\" to restart the process.", embed: _embed);
                            _buildStage++;
                            break;

                        case 3:
                            if (_message.Content.Equals("YES", StringComparison.OrdinalIgnoreCase))
                            {
                                IMessageChannel channel = _guild.GetChannel(_data.Configuration.MentorChannelId) as IMessageChannel;

                                FinalizeQuestionnaire();

                                await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the mentor channel");
                                await channel.SendMessageAsync(text: $"Submitted by {_message.Author.Mention}:", embed: _embed);
                            }
                            else if (_message.Content.Equals("NO", StringComparison.OrdinalIgnoreCase))
                            {
                                _collabChannel = CollabChannel.Unknown;
                                _hiring = HiringStatus.Unknown;
                                _fields = new ConcurrentDictionary<string, string>();
                                _buildStage = 0;

                                await _message.Author.TrySendMessageAsync($"**Process restarted!**\nPlease enter which channel you would like to post:\n1- Paid\n2- Hobby\n3- Gametest\n4- Mentor");
                            }
                            else
                            {
                                await _message.Author.TrySendMessageAsync("Input invalid. Reply with \"**Yes**\" to approve, or \"**No**\" to restart the process.");
                            }
                            break;
                    }
                }
                async Task HandleHiring()
                {
                    switch (_buildStage)
                    {
                        case 0:
                            _fields.TryAdd("areasOfInterest", _message.Content);
                            await _message.Author.TrySendMessageAsync("Add a description:");
                            _buildStage++;
                            break;

                        case 1:
                            _fields.TryAdd("description", _message.Content);
                            await _message.Author.TrySendMessageAsync("How much are you willing to pay? (*Ie. \"**Free**\", \"**5€/h**\", \"**10$/h ~ 30$/h**\"*)");
                            _buildStage++;
                            break;

                        case 2:
                            _fields.TryAdd("compensation", _message.Content);
                            foreach (var pair in _fields)
                            {
                                _fields[pair.Key] = SanitizeMarkdown(pair);
                            }
                            _embed = BuildMentorEmbed();
                            await _message.Author.TrySendMessageAsync("This will be sent to the mentor channel. Reply with \"**Yes**\" to approve, or \"**No**\" to restart the process.", embed: _embed);
                            _buildStage++;
                            break;

                        case 3:
                            if (_message.Content.Equals("YES", StringComparison.OrdinalIgnoreCase))
                            {
                                IMessageChannel channel = _guild.GetChannel(_data.Configuration.MentorChannelId) as IMessageChannel;

                                FinalizeQuestionnaire();

                                await _message.Author.TrySendMessageAsync("Complete! Your embed will be sent to the mentor channel");
                                await channel.SendMessageAsync(text: $"Submitted by {_message.Author.Mention}:", embed: _embed);
                            }
                            else if (_message.Content.Equals("NO", StringComparison.OrdinalIgnoreCase))
                            {
                                _collabChannel = CollabChannel.Unknown;
                                _hiring = HiringStatus.Unknown;
                                _fields = new ConcurrentDictionary<string, string>();
                                _buildStage = 0;

                                await _message.Author.TrySendMessageAsync($"**Process restarted!**\nPlease enter which channel you would like to post:\n1- Paid\n2- Hobby\n3- Gametest\n4- Mentor");
                            }
                            else
                            {
                                await _message.Author.TrySendMessageAsync("Input invalid. Reply with \"**Yes**\" to approve, or \"**No**\" to restart the process.");
                            }
                            break;
                    }
                }
            }

            private void FinalizeQuestionnaire()
            {
                _collab.UpdateCollabTimeout(_message.Author);
                _collab.DeactivateUser(_message.Author);                
            }

            private Embed BuildPaidNotHiringEmbed()
            {
                _fields["portfolio"] = MoveLinksToNewline();

                return new EmbedBuilder().WithTitle("Looking for Work")
                    .WithDescription(_fields["description"])
                    .WithAuthor(_message.Author)
                    .WithColor(Color.Blue)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("My Role", _fields["roles"], true)
                    .AddField("My Skills", _fields["skills"], true)
                    .AddField("My Portfolio", _fields["portfolio"], false)
                    .AddField("Experience in Field", _fields["experience"], true)
                    .AddField("My Rates", _fields["compensation"], true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build();
            }
            private Embed BuildPaidHiringEmbed()
            {
                _fields["portfolio"] = MoveLinksToNewline();

                return new EmbedBuilder().WithTitle("Hiring")
                    .WithDescription(_fields["description"])
                    .WithAuthor(_message.Author)
                    .WithColor(Color.Green)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("Project Title", _fields["projectName"], true)
                    .AddField("Role(s) required", _fields["roles"], true)
                    .AddField("Previous Projects/Portfolio", _fields["portfolio"], false)
                    .AddField("Current Team Size", _fields["teamSize"], true)
                    .AddField("Project Length", _fields["projectLength"], true)
                    .AddField("Compensation", _fields["compensation"], true)
                    .AddField("Responsibilities", _fields["responsibilities"], true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build();
            }
            private Embed BuildHobbyNotHiringEmbed()
            {
                _fields["portfolio"] = MoveLinksToNewline();

                return new EmbedBuilder().WithTitle("Looking for work")
                    .WithDescription(_fields["description"])
                    .WithAuthor(_message.Author)
                    .WithColor(Color.Blue)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("My Role(s)", _fields["roles"], true)
                    .AddField("My Skills", _fields["skills"], true)
                    .AddField("Previous Projects/Portfolio", _fields["portfolio"], false)
                    .AddField("Experience in the field", _fields["experience"], true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build();
            }
            private Embed BuildHobbyHiringEmbed()
            {
                _fields["portfolio"] = MoveLinksToNewline();

                return new EmbedBuilder().WithTitle("Hiring")
                    .WithDescription(_fields["description"])
                    .WithAuthor(_message.Author)
                    .WithColor(Color.Green)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("Project Title", _fields["projectName"], true)
                    .AddField("Role(s) Required", _fields["roles"], true)
                    .AddField("Previous Projects/Portfolio", _fields["portfolio"], false)
                    .AddField("Current Team Size", _fields["teamSize"], true)
                    .AddField("Project Length", _fields["projectLength"], true)
                    .AddField("Responsibilities", _fields["responsibilities"], true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build();
            }
            private Embed BuildGametestEmbed()
            {
                bool hasLink = _fields["link"] != "-" && _fields["link"] != "\"-\"";

                return new EmbedBuilder().WithTitle(_fields["projectName"])
                    .WithDescription(_fields["description"])
                    .WithAuthor(_message.Author)
                    .WithColor(Color.Orange)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("Platforms", _fields["platforms"], true)
                    .AddFieldConditional(hasLink, "Download Link", _fields["link"], true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build();
            }
            private Embed BuildMentorEmbed()
            {
                string title = _hiring == HiringStatus.Hiring ? "Looking for a mentor" : "Looking to mentor";
                Color color = _hiring == HiringStatus.Hiring ? Color.Green : Color.Blue;
                       
                return new EmbedBuilder().WithTitle(title)
                    .WithDescription(_fields["description"])
                    .WithAuthor(_message.Author)
                    .WithColor(color)
                    .WithThumbnailUrl(_message.Author.EnsureAvatarUrl())
                    .AddField("Areas of Interest", _fields["areasOfInterest"], true)
                    .AddField("Rates", _fields["compensation"], true)
                    .AddField("Contact via DM", MentionUtils.MentionUser(_message.Author.Id))
                    .Build();               
            }

            private string MoveLinksToNewline()
            {
                const string linkRegex = @"https?//|www\.|(\S\.\S)";
                const string httpsRegex = @"https?";

                string[] _portfolioArray = _fields["portfolio"].Split(' ');
                for (int i = 0; i < _portfolioArray.Length; i++)
                {
                    // Is current iteration a link?
                    if (Regex.Match(_portfolioArray[i], linkRegex).Success)
                    {
                        // Does current iteration include "https://" (required for the link to be displayed as hypertext in the embed)
                        // This may produce weird results with [hidden](link) formatting
                        if (!Regex.Match(_portfolioArray[i], httpsRegex).Success)
                        {
                            _portfolioArray[i] = $"https://{_portfolioArray[i]}";
                        }
                        _portfolioArray[i] = $"\n{_portfolioArray[i]}\n";
                    }
                }

                return string.Join(' ', _portfolioArray);
            }
            private static string SanitizeMarkdown(KeyValuePair<string, string> field)
            {
                string text = field.Value;
                if (field.Key == "portfolio")
                {
                    text = text.Replace("[", "\\[");
                    return text;
                }
                foreach (string unsafeChar in SensitiveCharacters)
                    text = text.Replace(unsafeChar, $"\\{unsafeChar}");
                return text;
            }
        }
    }
}
