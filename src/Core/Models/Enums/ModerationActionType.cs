using System.ComponentModel;

namespace BrackeysBot
{
    public enum ModerationActionType
    {
        [Description("Warned user")]
        Warn,
        [Description("Muted user")]
        Mute,
        [Description("Banned user")]
        Ban,
        [Description("Tempbanned user")]
        TempBan,
        [Description("Tempmuted user")]
        TempMute,
        [Description("Cleared messages")]
        ClearMessages,
        [Description("Set slowmode")]
        SlowMode,
        [Description("Unbanned user")]
        Unban,
        [Description("Unmuted user")]
        Unmute,
        [Description("Kicked user")]
        Kick,
        [Description("Filtered word")]
        Filtered
    }
}
