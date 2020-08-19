using System;
using System.Threading.Tasks;

using Discord.Commands;

namespace BrackeysBot.Core.Models
{
    [Name("Category")]
    [Summary("Organizes the command into the specified category.")]
    public class CategoryFeature : CustomCommandFeature
    {
        public string Category { get; set; }

        public override void FillArguments(string arguments)
        {
            if (arguments.Equals("uncategorized", StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("Invalid category name.");

            Category = arguments.ToLower();
        }
        public override Task Execute(ICommandContext context)
            => Task.CompletedTask;

        public override string ToString()
            => $"Organized into **{Category}**.";
    }
}
