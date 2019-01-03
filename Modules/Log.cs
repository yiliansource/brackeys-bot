using System;
using System.IO;

namespace BrackeysBot
{
    /// <summary>
    /// Provides the utility of logging messages to a persistant file.
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Stores settings for the logger.
        /// </summary>
        public class LogSettings
        {
            /// <summary>
            /// Should the logged messages include a timestamp?
            /// </summary>
            public bool IncludeTimestamp { get; set; } = true;
        }

        /// <summary>
        /// The settings that the logger will run with.
        /// </summary>
        public static LogSettings Settings { get; set; }

        private static string _logDirectory;
        
        /// <summary>
        /// Initializes the logger.
        /// </summary>
        public static void Initialize()
        {
            // If no settings have been set yet, initialize them to their default values
            if (Settings == null) Settings = new LogSettings();

            _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "log");

            // If the log directory doesn't exist, create it
            if (!Directory.Exists(_logDirectory))
            {
                Console.WriteLine("Log directory doesn't exist yet. Creating it ...");
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public static void WriteLine(object obj) => WriteLine(obj.ToString());
        public static void WriteLine(string message) => Write(message + Environment.NewLine);
        public static void Write(object obj) => Write(obj.ToString());
        public static void Write(string message)
        {
            if (Settings.IncludeTimestamp) message = $"[{DateTime.Now.ToString("HH:mm:ss")}] " + message;

            Console.Write(message);
            AppendToCurrentLogfile(message);
        }

        /// <summary>
        /// Appens the message to the current logfile.
        /// </summary>
        private static void AppendToCurrentLogfile(string message)
        {
            string filepath = GetCurrentLogfilePath();

            File.AppendAllText(filepath, message);
        }
        /// <summary>
        /// Creates a logfile for the specified date.
        /// </summary>
        private static void CreateLogfileForDay(DateTime date)
        {
            string content = $"( Logfile {date.ToString("dd.MM.yyyy")} )" + Environment.NewLine;
            string path = GetLogfilePathForDate(date);
            File.WriteAllText(path, content);
        }
        /// <summary>
        /// Returns the path of the current logfile. If no such file exists yet, it will be created.
        /// </summary>
        public static string GetCurrentLogfilePath()
        {
            string filepath = GetLogfilePathForDate(DateTime.Now);
            
            if (!File.Exists(filepath))
                CreateLogfileForDay(DateTime.Now);

            return filepath;
        }
        /// <summary>
        /// Returns the logfile path for the specified date.
        /// </summary>
        public static string GetLogfilePathForDate(DateTime date)
            => Path.Combine(_logDirectory, GetLogfileNameForDate(date));
        /// <summary>
        /// Returns the logfile name for the specified date.
        /// </summary>
        private static string GetLogfileNameForDate(DateTime date)
            => $"log_{date.ToString("dd-MM-yyyy")}.txt";
    }
}
