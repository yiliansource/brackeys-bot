using System.Linq;
using System.Text;

using Discord;

namespace BrackeysBot.Commands.Archive
{
    /// <summary>
    /// Represents an event submission, as an <see cref="ArchiveMessage"/>.
    /// </summary>
    internal class ArchiveEventSubmission : ArchiveMessage
    {
        private readonly string _voteEmote;
        
        public ArchiveEventSubmission(IMessage original, string voteEmote) : base(original)
        {
            _voteEmote = voteEmote;
        }

        /// <summary>
        /// Formats the submission into a string.
        /// </summary>
        public override string Format()
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

            // Include the votes
            if (!string.IsNullOrEmpty(_voteEmote))
            {
                IUserMessage message = Original as IUserMessage;
                if (message != null)
                {
                    var kvp = message.Reactions.FirstOrDefault(r => r.Key.Name == _voteEmote);
                    int votes = kvp.Value.ReactionCount;

                    if (votes > 0)
                    {
                        sb.AppendLine().AppendLine($"**Votes: {votes}**");
                    }
                }
            }

            return sb.ToString();
        }
    }
}
