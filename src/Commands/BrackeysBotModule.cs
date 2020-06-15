using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

using Discord;
using Discord.Commands;

using BrackeysBot.Services;
using BrackeysBot.Managers;

namespace BrackeysBot.Commands
{
    public abstract class BrackeysBotModule : ModuleBase<BrackeysBotContext>
    {
        public DataService Data { get; set; }
        public InfractionManager Infractions { get; set; }
        public ChatManager Chat { get; set; }
        public ModerationLogService ModerationLog { get; set; }

        protected EmbedBuilder GetDefaultBuilder()
            => new EmbedBuilder()
                .WithColor(this.GetType().GetCustomAttribute<ModuleColorAttribute>()?.Color ?? Color.DarkerGrey);
    }
}
