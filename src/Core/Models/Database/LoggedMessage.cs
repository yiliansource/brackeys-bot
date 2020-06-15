using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrackeysBot.Models.Database
{
    public class LoggedMessage
    {
        [Key, Column("msg_id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MsgId { get; set; }
        [Column("user_id")]
        public ulong UserId { get; set; }
        [Column("dchannel_id")]
        public ulong DiscordChannelId { get; set; }
        [Column("dmsg_id")]
        public ulong DiscordMessageId { get; set; }
        [Column("msg_action")]
        public int MsgAction { get; set; }
        [Column("msg_c1")]
        public string MsgContent1 { get; set; }
        [Column("msg_c2")]
        public string MsgContent2 { get; set; }
    }
}