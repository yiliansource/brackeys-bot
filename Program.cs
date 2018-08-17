using System.Threading.Tasks;

namespace BrackeysBot
{
    internal sealed class Program
    {
        public static async Task Main(string[] args)
        {
            var bot = new BrackeysBot();
            await bot.Start();
            await Task.Delay(-1);
        }
    }
}