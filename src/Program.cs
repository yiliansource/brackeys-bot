using System.Threading.Tasks;

namespace BrackeysBot
{
    public class Program
    {
        public static async Task Main(string[] args)
            => await new BrackeysBot().RunAsync();
    }
}
