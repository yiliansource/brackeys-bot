using System;
using BrackeysBot.Core.Models;
using Discord;
using Discord.WebSocket;

namespace BrackeysBot
{
    public static class DiscordExtensions
    {
        public static GuildUserProxy TryGetUser(this DiscordSocketClient client, ulong guildId, ulong userId) 
        {
            GuildUserProxy proxy = new GuildUserProxy {
                ID = userId
            };

            try 
            {
                proxy.GuildUser = client.GetGuild(guildId).GetUser(userId) as IGuildUser;
            } 
            catch (Exception ignore) 
            {

            }

            return proxy;
        }
    }
}
