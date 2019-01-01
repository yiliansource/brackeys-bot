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
            // The base format
            StringBuilder sb = new StringBuilder(base.Format());

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
