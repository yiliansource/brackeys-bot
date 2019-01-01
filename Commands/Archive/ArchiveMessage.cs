using System.Linq;
using System.Text;
using System.Collections.Generic;

using Discord;

namespace BrackeysBot.Commands.Archive
{
    /// <summary>
    /// Represents a message that can be stored in an archive.
    /// </summary>
    internal class ArchiveMessage
    {
        /// <summary>
        /// The message that the <see cref="ArchiveMessage"/> was created from.
        /// </summary>
        public IMessage Original { get; }

        public ArchiveMessage(IMessage original)
        {
            Original = original;
        }

        /// <summary>
        /// Returns all images that are attached to the message, in the form of <see cref="ArchiveMessage/>s.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<ArchiveImage> GetMessageImages()
            => Original.Attachments.Select(a => new ArchiveImage(a));

        /// <summary>
        /// Formats the message to a string.
        /// </summary>
        public virtual string Format()
        {
            // The header, including the author, and date
            StringBuilder sb = new StringBuilder()
                .AppendLine($"{Original.Author.Username}#{Original.Author.Discriminator} ({Original.Author.Id}) - ({Original.CreatedAt.ToString("dd/MM/yyyy")})")
                .AppendLine();

            // If the message has a content, include it
            if (!string.IsNullOrWhiteSpace(Original.Content))
                sb.AppendLine($"{Original.Content}{(Original.EditedTimestamp.HasValue ? " - _(edited)_" : "")}");

            // Include every attached image in the message
            foreach (ArchiveImage image in GetMessageImages())
                sb.AppendLine($"![{image.Identifier}](assets/{image.Identifier})");

            return sb.ToString();
        }
    }
}