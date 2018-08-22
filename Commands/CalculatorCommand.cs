using System.Threading.Tasks;
using System;

using Discord;
using Discord.Commands;

using Flee.PublicTypes;

namespace BrackeysBot.Commands
{
    public class CalculatorCommand : ModuleBase
    {
        [Command ("calc")]
        [Alias ("calculator", "math")]
        [HelpData("calculator <expression>", "Calculates the given mathematical expression.")]
        public async Task Calculator([Remainder]string expression)
        {
            ExpressionContext context = new ExpressionContext();
            context.Imports.AddType(typeof(Math));
            IDynamicExpression e = context.CompileDynamic(expression);
            await ReplyAsync($"The result is {e.Evaluate()}");
        }
    }
}