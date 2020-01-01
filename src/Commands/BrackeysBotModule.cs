using System;
using System.Collections.Generic;
using System.Text;

using Discord;
using Discord.Commands;

using BrackeysBot.Services;

namespace BrackeysBot.Commands
{
    public abstract class BrackeysBotModule : ModuleBase<BrackeysBotContext>
    {
        public DataService Data { get; set; }
        public ModerationLogService ModerationLog { get; set; }
    }
}
