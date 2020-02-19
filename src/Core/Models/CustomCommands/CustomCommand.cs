using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Core.Models
{
    public class CustomCommand
    {
        public string Name { get; set; }
        public List<CustomCommandFeature> Features { get; set; }

        private CustomCommand() { }
        public CustomCommand(string name)
        {
            Name = name;
            Features = new List<CustomCommandFeature>();
        }

        public bool Matches(string name)
        {
            if (Features.Find(c => c is AliasFeature) is AliasFeature alias 
                && alias.Matches(name)) 
                return true;

            return Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
        }

        public virtual async Task ExecuteCommand(ICommandContext context)
        {
            if (Features.Count > 0)
            {
                foreach (CustomCommandFeature feature in Features)
                {
                    await feature.Execute(context);
                }
            }
            else
            {
                await context.Channel.SendMessageAsync(string.Empty, false, new EmbedBuilder().WithDescription("No features have been set yet!").Build());
            }
        }
    }
}
