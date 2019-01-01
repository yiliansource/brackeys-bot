using Discord;

namespace BrackeysBot.Commands.Archive
{
    /// <summary>
    /// Represents an image that can be stored in an archive.
    /// </summary>
    internal class ArchiveImage
    {
        /// <summary>
        /// The message that the <see cref="ArchiveImage"/> was created from.
        /// </summary>
        public IAttachment Original { get; }

        public ArchiveImage(IAttachment original)
        {
            Original = original;
        }

        /// <summary>
        /// The identifier of the image.
        /// </summary>
        public string Identifier => Original.Filename;
        /// <summary>
        /// The URL that the image is located at.
        /// </summary>
        public string URL => Original.ProxyUrl;
    }
}