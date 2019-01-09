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

        private const string branch = "origin master";

        private const string upToDate = "Already up to date.\n";

        public VersioningCommand(BrackeysBot bot)
        {
            _bot = bot;
        }

        [Command("update")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("update", "Updates the bot by pulling from git.")]
        public async Task Update()
        {
            await ReplyAsync("Updating! :arrow_down:");
            Log.WriteLine ("Updating...");

            var cmd = RunShellScript("git", true, $"pull {branch}");

            string output = await cmd.StandardOutput.ReadToEndAsync ();
            Log.WriteLine (output);

            if (output.Substring(output.Length - upToDate.Length) != upToDate) // If the output doesn't end with "Already up to date.\n"
            {
                await ReplyAsync ("Restarting the bot... :arrows_counterclockwise: ");
                Log.WriteLine ("Restarting the bot...");

                // Create a file that will be checked on startup to check if the bot updated
                // The channel ID is used so the bot knows where to send the updated message on startup
                await File.WriteAllTextAsync (Path.Combine (Directory.GetCurrentDirectory (), "updated.txt"), Context.Guild.Id + "\n" + Context.Channel.Id);
                
                RunShellScript("dotnet", false, "run");

                // Cleanly shutdown the bot
                await _bot.ShutdownAsync(true);
            }
            else
            {
                await ReplyAsync("The newest version is already installed! :white_check_mark:");
            }
        }

        /// <summary>
        /// Runs a command.
        /// </summary>
        /// <param name="command">The command to execute, without the extension.</param>
        /// <param name="redirectStdout">Whether to redirect standard output to this program.</param>
        /// <param name="args">Arguments that should be passed to the executing script.</param>
        /// <returns>Returns the ran process.</returns>
        private static Process RunShellScript (string command, bool redirectStdout, string args)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = redirectStdout,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Directory.GetCurrentDirectory ()
            };

            psi.FileName = $"{command}";
            psi.Arguments = args;

            Process cmd = Process.Start (psi);
            return cmd;
        }
    }
}
