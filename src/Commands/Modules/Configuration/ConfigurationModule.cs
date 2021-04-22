using Discord;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
	public partial class ConfigurationModule : BrackeysBotModule
	{
		private DiscordSocketClient _socketClient;
		private IUserMessage _lastConfigMessage;
		private int _currentPage;
		
		public ConfigurationModule(DiscordSocketClient socketClient)
		{
			_socketClient = socketClient;
		}
	}
}
