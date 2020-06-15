using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;
using Humanizer;

namespace BrackeysBot.Models.Database
{
    [Table("infractions_messages")]
    public class InfractionsMessages
    {
        [Key, Column("c_id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column("infraction_id"), Required]
        public int InfractionId { get; set; }
        [Column("msg_id"), Required]
        public int MessageId { get; set; }
    }
}