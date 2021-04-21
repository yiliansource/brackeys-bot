using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using BrackeysBot.Services;

namespace BrackeysBot.Commands
{
    public partial class ConfigurationModule : BrackeysBotModule
    {
        public ConfigurationService Config { get; set; }

        [Command("config"), Alias("configuration", "c")]
        [Remarks("config [name] [value]")]
        [Summary("Shows the entire configuration, or just a single value, or changes a value.")]
        [RequireModerator]
        public async Task DisplayConfigAllAsync()
        {
			EmbedBuilder builder = ConfigEmbedBuilder();

			_lastConfigMessage = await Context.Channel.SendMessageAsync(string.Empty, false, builder.Build());
			var emojis = new Emoji[] { new Emoji("\U00002B05"), new Emoji("\U000027A1") };  // Unicode characters for left and right arrows, respectively
            _currentPage = 0;
			await _lastConfigMessage.AddReactionsAsync(emojis);

			_discord.ReactionAdded += HandleReactionAddedAsync;
		}

		public async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel originChannel, SocketReaction reaction)
		{
            var message = await cachedMessage.GetOrDownloadAsync();
            if (message == null || 
                reaction.UserId == message.Author.Id ||
                message.Id != _lastConfigMessage?.Id)
			{
				return;
			}

            var left = new Emoji("\U00002B05").Emotify();
            var right = new Emoji("\U000027A1").Emotify();
            if (reaction.Emote.Emotify() == left)
            {
                _currentPage--;
                var builder = ConfigEmbedBuilder();
                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                await message.ModifyAsync(p => p.Embed = builder.Build());
            }
            else if (reaction.Emote.Emotify() == right)
			{
                _currentPage++;
                var builder = ConfigEmbedBuilder();
                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                await message.ModifyAsync(p => p.Embed = builder.Build());
			}
		}

        private EmbedBuilder ConfigEmbedBuilder()
        {          
            EmbedBuilder builder = GetDefaultBuilder().WithTitle("Configuration");
            var values = Config.GetConfigurationValues().ToList();

            const int maxFieldsPerPage = 25;
            var maxPages = (int) MathF.Ceiling((float) values.Count / maxFieldsPerPage);
			
            if (_currentPage < 0)
			{
                _currentPage = maxPages - 1;
			}
            else if(_currentPage >= maxPages)
			{
                _currentPage = 0;
			}

			for (int i = 0; i < maxFieldsPerPage; i++)
            {
                var valueIndex = _currentPage * maxFieldsPerPage + i;
                if (valueIndex >= values.Count)
                {
                    break;
                }

                builder.AddField(new EmbedFieldBuilder()
                    .WithName(values[valueIndex].Name)
                    .WithValue(LimitFieldLength(values[i].ToString()))
                    .WithIsInline(true));
            }

            return builder;
        }

        [Command("config"), Alias("configuration", "c", "viewconfig")]
        [Remarks("config <name>")]
        [Summary("Displays information about a value in the configuration.")]
        [RequireModerator]
        [HideFromHelp]
        public async Task DisplayConfigAsync(
            [Summary("The name of the configuration entry.")] string name)
        {
            EmbedBuilder builder = GetDefaultBuilder();

            if (Config.TryGetValue(name, out ConfigurationValue configValue))
            {
                builder.WithTitle(configValue.Name)
                    .WithDescription(configValue.Description)
                    .AddField("Value", LimitFieldLength(configValue.ToString()));
            }
            else
            {
                builder.WithDescription($"A configuration entry with the name of {name} could not be found.")
                    .WithColor(Color.DarkRed);
            }

            await builder.Build().SendToChannel(Context.Channel);
        }

        [Command("config"), Alias("configuration", "c", "setconfig")]
        [Remarks("config <name> <value>")]
        [Summary("Sets the value of a configuration.")]
        [RequireModerator]
        [HideFromHelp]
        public async Task SetConfigAsync(
            [Summary("The name of the configuration entry.")] string name, 
            [Summary("The value to set the entry to."), Remainder] string value)
        {
            EmbedBuilder builder = GetDefaultBuilder();

            if (Config.TryGetValue(name, out ConfigurationValue configValue))
            {
                if (configValue.SetValue(value))
                {
                    builder.WithDescription($"Successfully set **{configValue.Name}** to `{value}`.")
                        .WithColor(Color.Green);
                }
                else
                {
                    builder.WithDescription($"Something went wrong while changing the value of {configValue.Name}.")
                        .WithColor(Color.Red);
                }
            }
            else
            {
                builder.WithDescription($"A configuration entry with the name of {name} could not be found.")
                    .WithColor(Color.DarkRed);
            }

            await builder.Build().SendToChannel(Context.Channel);
        }

        [Command("saveconfig"), Alias("saveconfiguration", "sc")]
        [Summary("Saves the configuration of the bot to the config file.")]
        [RequireModerator]
        public async Task SaveConfigAsync()
        {
            Config.Save();

            await GetDefaultBuilder()
                .WithDescription("Configuration was saved!")
                .Build()
                .SendToChannel(Context.Channel);
        }

        private static string LimitFieldLength(string content)
        {
            const int maxLength = EmbedFieldBuilder.MaxFieldValueLength;
            const string truncator = "[...]";
            if (content.Length > maxLength)
            {
                content = content[0..(maxLength - truncator.Length)] + truncator;
            }
            return content;
        }
    }
}
