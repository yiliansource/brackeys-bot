using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

using Discord;

namespace BrackeysBot.Modules
{
    /// <summary>
    /// Represents a handler that manages the moderation of the video suggestions channel.
    /// </summary>
    public sealed class VideoSuggestionsHandler
    {
        /// <summary>
        /// The channel that the moderation should be applied to.
        /// </summary>
        public IGuildChannel VideoSuggestionsChannel { get; set; }
        /// <summary>
        /// The role that is affected by the moderation. @everyone by default.
        /// </summary>
        public IRole DefaultRole { get; set; }

        private static readonly DateTime _initialOpen = new DateTime(2019, 4, 1, 0, 0, 0); // 1st of April, 2019
        private const int _openMonthCount = 4;
        private const int _closedMonthCount = 2;

        private readonly IGuild _guild;

        private const ulong READ_MESSAGE_PERMISSION = 1024u;
        private const string EVERYONE = "@everyone";
        private const string CHANNEL_NAME = "video-suggestions";

        public VideoSuggestionsHandler(IGuild guild)
        {
            _guild = guild;
        }
        /// <summary>
        /// Initializes the default values of the handler, in respect to the guild.
        /// </summary>
        public async Task InitializeAsync()
        {
            DefaultRole = _guild.Roles.First(r => r.Name.Equals(EVERYONE));
            VideoSuggestionsChannel = (await _guild.GetTextChannelsAsync()).FirstOrDefault(c => c.Name.Equals(CHANNEL_NAME));
        }

        /// <summary>
        /// Is the current state of the channel in respect to the time valid?
        /// </summary>
        public bool IsChannelStateValid()
        {
            if (VideoSuggestionsChannel == null)
                throw new ArgumentException($"The {nameof(VideoSuggestionsChannel)} has not been assigned.");

            bool isOpen = IsChannelOpen();
            bool shouldBeOpen = ShouldChannelBeOpen();

            return isOpen == shouldBeOpen;
        }
        /// <summary>
        /// Updates the state of the channel to be valid.
        /// </summary>
        public async Task UpdateChannelStateToValid()
        {
            await UpdateChannelState(ShouldChannelBeOpen());
        }
        /// <summary>
        /// Updates the state of the channel to the specified state.
        /// </summary>
        public async Task UpdateChannelState(bool open)
        {
            OverwritePermissions permissions = VideoSuggestionsChannel.GetPermissionOverwrite(DefaultRole).Value;

            ulong allow = permissions.AllowValue;
            ulong deny = permissions.DenyValue;

            bool isOpen = (allow & READ_MESSAGE_PERMISSION) > 0;

            if (isOpen == open)
                return;

            allow ^= READ_MESSAGE_PERMISSION;
            deny ^= READ_MESSAGE_PERMISSION;
            
            await VideoSuggestionsChannel.AddPermissionOverwriteAsync(DefaultRole, new OverwritePermissions(allow, deny));
        }

        private bool IsChannelOpen()
        {
            OverwritePermissions? permissions = VideoSuggestionsChannel.GetPermissionOverwrite(DefaultRole);
            if (!permissions.HasValue)
            {
                OverwritePermissions defaultPerms = new OverwritePermissions(READ_MESSAGE_PERMISSION, 0);
                VideoSuggestionsChannel.AddPermissionOverwriteAsync(DefaultRole, defaultPerms);
                return true;
            }
            else
            {
                return permissions.Value.ReadMessages != PermValue.Deny;
            }
        }
        private bool ShouldChannelBeOpen()
        {
            DateTime current = DateTime.UtcNow;
            int monthsSinceInitialOpen = (current.Month - _initialOpen.Month) + 12 * (current.Year - _initialOpen.Year);
            int cycle = monthsSinceInitialOpen % (_openMonthCount + _closedMonthCount);
            
            return cycle <= _openMonthCount;
        }
    }
}
