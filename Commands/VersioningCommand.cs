using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands
{
    public class VersioningCommand : ModuleBase
    {
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public static bool CheckNeedsUpdate()
        {
            var cmd = RunShellScript("shell/checkversion.sh");

            string result = cmd.StandardOutput.ReadLine();
            cmd.WaitForExit();
            
            switch (result)
            {
                case "Up-to-date": return false;
                default: return true;
            }
        }

        [Command("version")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("version", "Checks if an update for the bot is available.")]
        public async Task Version()
        {
            var cmd = RunShellScript("shell/checkversion.sh");
            
            string result = cmd.StandardOutput.ReadLine();
            cmd.WaitForExit();

            string reply;
            switch(result)
            {
                case "Up-to-date": reply = "The newest version is installed!"; break;
                case "Need to pull": reply = "An update is available! Do `[]update` to get it!"; break;

                default: reply = "Something went wrong while checking the version. Please inform a staff member!"; break;
            }

            await ReplyAsync(reply);
        }

        [Command("update")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("update", "Updates the bot by pulling from git.")]
        public async Task Update()
        {
            if (!CheckNeedsUpdate())
            {
                await ReplyAsync("The newest version is already installed! :white_check_mark:");
            }
            else
            {
                await ReplyAsync("Updating! :arrow_down:");

                string pid = Process.GetCurrentProcess().Id.ToString();
                var cmd = RunShellScript("shell/update.sh", pid);
            }
        }


        private static Process RunShellScript(string shellscript, params string[] args)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C sh { shellscript } { string.Join(' ', args) }",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = false,
                UseShellExecute = false,
                WorkingDirectory = System.IO.Directory.GetCurrentDirectory()
            };

            Process cmd = Process.Start(psi);
            return cmd;
        }
    }
}
