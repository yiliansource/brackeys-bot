using Discord;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
	public partial class ConfigurationModule : BrackeysBotModule
	{
        private readonly DiscordSocketClient _discord;
        private IUserMessage _lastConfigMessage;
        private int _currentPage;

		ConfigurationModule(DiscordSocketClient discord)
        {
            _discord = discord;
        }
    }
}
