using SysVer = System.Version;

namespace BrackeysBot
{
    public static class Version
    {
        public const int MAJOR = 3;
        public const int MINOR = 0;
        public const int PATCH = 18;
        public const int HOTFIX = 3;

        public static string ShortVersion = $"{MAJOR}.{MINOR}";
        public static string FullVersion => $"{MAJOR}.{MINOR}.{PATCH}.{HOTFIX}";
        public static string DiscordVersion => Discord.DiscordConfig.Version;
        public static SysVer AsSystemVersion() => new SysVer(MAJOR, MINOR, PATCH, HOTFIX);
    }
}
