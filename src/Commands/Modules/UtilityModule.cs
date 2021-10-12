using System;
using System.Collections.Generic;
using System.Text;

using Discord.Commands;
﻿using BrackeysBot.Services;

namespace BrackeysBot.Commands
{
    [ModuleColor(0xa2e089)]
    public partial class UtilityModule : BrackeysBotModule
    {
        public FilterService FilterService { get; set; }
        
        public MathService MathService { get; set; }

        public FormatCodeService FormatCodeService { get; set; }

        public CollabService CollabService { get; set; }
    }
}
