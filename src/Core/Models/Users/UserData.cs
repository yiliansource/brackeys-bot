using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BrackeysBot
{
    public class UserData
    {
        [JsonPropertyName("id")]
        public ulong ID { get; set; }
        [JsonPropertyName("temporaryInfractions")]
        public List<TemporaryInfraction> TemporaryInfractions { get; set; } = new List<TemporaryInfraction>();
        [JsonPropertyName("infractions")]
        public List<Infraction> Infractions { get; set; } = new List<Infraction>();

        public UserData (ulong id)
        {
            ID = id;
        }
        private UserData()
        {

        }
    }
}
