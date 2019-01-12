using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static BrackeysBot.ProcessHelper;

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

        private const string separator = "-> ";

        public static readonly string pidPath = Path.Combine(Directory.GetCurrentDirectory(), "pid.txt");

        private static Process child;

        public VersioningCommand(BrackeysBot bot)
        {
            _bot = bot;
        }

        [Command("update")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("update", "Updates the bot by pulling from git.")]
        public async Task Update()
        {
            // Cleanly shutdown the bot
            if (!_bot.OriginalProcess)
                await _bot.Shutdown(true);
            else
            {
                await ReplyAsync("Updating! :arrow_down:");
                Log.WriteLine("Updating...");

                var cmd = RunShellScript("git", true, $"pull {branch}");

                string output = await cmd.StandardOutput.ReadToEndAsync();
                Log.WriteLine(output);
                cmd.Dispose();

                if (!output.EndsWith(upToDate)) // If the output doesn't end with "Already up to date.\n"
                {
                    await ReplyAsync("Restarting the bot... :arrows_counterclockwise: ");
                    Log.WriteLine("Restarting the bot...");

                    #region Build Bot
                    var buildProcess = RunShellScript("dotnet", true, $"build -c Release");

                    // Find build directory using the BrackeysBot -> path message dotnet build prints
                    var buildMessages = await buildProcess.StandardOutput.ReadToEndAsync();
                    var separatorPos = buildMessages.IndexOf(separator) + separator.Length;
                    if (separatorPos == -1 + separator.Length) // Build failed, BrackeysBot -> path not found
                    {
                        await ReplyAsync("Compiling the bot failed.");
                        return;
                    }
                    var newlinePos = buildMessages.IndexOf(Environment.NewLine, separatorPos);
                    var exportDir = buildMessages.Substring(separatorPos, newlinePos - separatorPos);
                    Log.WriteLine(buildMessages);
                    Log.WriteLine($"Binary found at: {exportDir}");

                    string buildProcessID = buildProcess.Id.ToString();
                    buildProcess.Dispose();
                    #endregion

                    child = RunShellScript("dotnet", true, exportDir);
                    child.EnableRaisingEvents = true;
                    child.Exited += new EventHandler(ChildClosed);
                    child.RedirectToConsoleOutput();

                    await File.WriteAllTextAsync(pidPath, child.Id.ToString());


                    // Create a file that will be checked on startup to check if the bot updated
                    // The channel ID is used so the bot knows where to send the updated message on startup
                    await File.WriteAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "updated.txt"), Context.Guild.Id + "\n" + Context.Channel.Id);
                    AppDomain.CurrentDomain.ProcessExit += new EventHandler(CloseChild);
                    Console.CancelKeyPress += new ConsoleCancelEventHandler(CloseChild);

                    if (_bot._client.ConnectionState == ConnectionState.Connected)
                        await _bot.Shutdown(false);
                }
                else
                {
                    await ReplyAsync("The newest version is already installed! :white_check_mark:");
                }
            }
        }

        /// <summary>
        /// Runs a command.
        /// </summary>
        /// <param name="command">The command to execute, without the extension.</param>
        /// <param name="redirectStdout">Whether to redirect standard output to this program.</param>
        /// <param name="args">Arguments that should be passed to the executing script.</param>
        /// <returns>Returns the ran process.</returns>
        private static Process RunShellScript(string command, bool redirectStdout, string args)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = redirectStdout,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Directory.GetCurrentDirectory()
            };

            psi.FileName = $"{command}";
            psi.Arguments = args;

            Process cmd = Process.Start(psi);
            return cmd;
        }

        // If the original process is being quit, the whole application should close gracefully, even if it's gone into "wrapper" mode
        public static void CloseChild(object sender, EventArgs e)
        {
            try
            {
                if (child != null)
                    child.EnableRaisingEvents = false;
                ProcessHelper.KillAndDispose(int.Parse(File.ReadAllText(pidPath)));
                File.Delete(pidPath);
            }
            catch { }
        }

        public async void ChildClosed(object sender, EventArgs e)
        {
            await Update();
        }
    }
}
