using System.Diagnostics;
using System.Threading.Tasks;
using System;

using Discord;
using Discord.Commands;

namespace BrackeysBot
{
    /// <summary>
    /// Provides extension methods for Processes.
    /// </summary>
    public static class ProcessHelper
    {
        public static void KillAndDispose(int pid)
        {
            Process process = Process.GetProcessById(pid);
            process.Kill();
            process.Dispose();
        }

        public static void KillAndDispose(this Process process)
        {
            process.Kill();
            process.Dispose();
        }
        public static void RedirectToConsoleOutput(this Process process)
        {
            _ = process.StandardOutput.BaseStream.CopyToAsync(Console.OpenStandardOutput());
        }
    }
}
