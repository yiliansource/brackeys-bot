using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Discord;
using Discord.Commands;
using System.Reflection;

namespace BrackeysBot.Commands
{
    public class VersioningCommand : ModuleBase
    {
        private BrackeysBot _bot;

        public VersioningCommand(BrackeysBot bot)
        {
            _bot = bot;
        }

        [Command("version")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("version", "Checks if an update for the bot is available.")]
        public async Task Version()
        {
            var cmd = RunShellScript("shell/checkversion", true, "origin/master");
            
            string result = cmd.StandardOutput.ReadLine ();
            cmd.WaitForExit();

            string reply;
            switch(result)
            {
                case "Up-to-date": reply = "The newest version is installed!"; break;
                case "Need to pull": reply = "An update is available! Do `[]update` to get it!"; break;

                default: reply = "Something went wrong while checking the version. Please inform a staff member!"; break;
            }

            Log.WriteLine (reply);

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
                Log.WriteLine ("Updating...");

                string pid = Process.GetCurrentProcess().Id.ToString();
                var cmd = RunShellScript("shell/update", true, pid);
                cmd.WaitForExit ();
                Log.WriteLine (cmd.StandardOutput.ReadToEnd ());

                await ReplyAsync ("Restarting the bot... :arrows_counterclockwise: ");
                Log.WriteLine ("Restarting the bot...");

                // Create a file that will be checked on startup to check if the bot updated
                // The channel ID is used so the bot knows where to send the updated message on startup
                await File.WriteAllTextAsync (Path.Combine (Directory.GetCurrentDirectory (), "updated.txt"), Context.Guild.Id + "\n" + Context.Channel.Id);
                
                Process.Start ("dotnet", "run " + Assembly.GetExecutingAssembly ().GetName ().CodeBase);

                // Cleanly shutdown the bot
                await _bot.ShutdownAsync(true);
            }
        }

        public static bool CheckNeedsUpdate()
        {
            var cmd = RunShellScript("shell/checkversion", true, "origin/master");

            string result = cmd.StandardOutput.ReadLine();
            cmd.WaitForExit();

            switch (result)
            {
                case "Up-to-date": return false;
                default: return true;
            }
        }

        /// <summary>
        /// Runs a shell script. Executes the script properly depending on the OS Platform.
        /// </summary>
        /// <param name="shellscript">The script to execute. Without the extensions. It automatically appends .sh or .bat depending on the OS.</param>
        /// <param name="redirectStdout">Whether to redirect standard output to this program.</param>
        /// <param name="args">Arguments that should be passed to the executing script.</param>
        /// <returns>Returns the ran process.</returns>
        private static Process RunShellScript (string shellscript, bool redirectStdout, params string [] args)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = redirectStdout,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Directory.GetCurrentDirectory ()
            };

            if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows))
            {
                // Convert to Windows style path
                shellscript = shellscript.Replace ("/", @"\");
                psi.FileName = "cmd.exe";
                psi.Arguments = $"/c {shellscript}.bat {string.Join (' ', args)}";
            }
            else if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux))
            {
                psi.FileName = "/bin/sh";
                psi.Arguments = $"{shellscript}.sh {string.Join (' ', args)}";
            }
            else
            {
                throw new PlatformNotSupportedException ("Your platform doesn't support checking the bot version or updating. Only supported on Windows and Linux.");
            }

            Process cmd = Process.Start (psi);
            return cmd;
        }
    }
}
