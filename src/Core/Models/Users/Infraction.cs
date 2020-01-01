using System;
using System.Text.Json.Serialization;

namespace BrackeysBot
{
    public struct Infraction
    {
        [JsonPropertyName("moderator")]
        public ulong Moderator { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("time")]
        public DateTime Time { get; set; }

        public static Infraction Create(ulong moderator, string description)
            => new Infraction(moderator, description);

        private Infraction(ulong moderator, string description)
        {
            Moderator = moderator;
            Description = description;
            Time = DateTime.Now;
        }
    }
}
