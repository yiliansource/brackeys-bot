using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands
{
    public class LogfileCommand : ModuleBase
    {
        [Command("logfile")]
        [PermissionRestriction(UserType.Staff)]
        public async Task GetLogfile()
        {
            string logfilePath = Log.GetLogfilePathForDate(DateTime.Now);
            if (File.Exists(logfilePath))
            {
                await Context.Channel.SendFileAsync(logfilePath, "Here's the logfile for today!");
            }
            else
            {
                await ReplyAsync("It seems like a logfile for today does not exist!");
            }
        }

        [Command("logfile")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("logfile <date>", "Fetches the logfile for a specific date.")]
        public async Task GetLogfile(string dateString)
        {
            DateTime date;
            if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", new CultureInfo("en-US"), DateTimeStyles.None, out date))
            {
                string logfilePath = Log.GetLogfilePathForDate(date);
                if (File.Exists(logfilePath))
                {
                    await Context.Channel.SendFileAsync(logfilePath, $"Here's the logfile for {date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.");
                }
                else
                {
                    await ReplyAsync($"It seems like a logfile for {date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} does not exist!");
                }
            }
            else
            {
                throw new Exception($"`{dateString}` is not a valid date. Please input the date in the format `dd/mm/yyyy`.");
            }
        }
    }
}
