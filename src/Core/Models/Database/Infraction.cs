using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;
using Humanizer;

namespace BrackeysBot.Models.Database
{
    [Table("infractions")]
    public class Infraction
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column("date")]
        public DateTime Date { get; set; }
        [Column("reason")]
        public string Reason { get; set; }
        [Column("moderation_types_type_id")]
        public int ModerationTypeId { get; set; }
        [Column("target_user_id")]
        public ulong TargetUserId { get; set; }
        [Column("moderator_user_id")]
        public ulong ModeratorUserId { get; set; }

        [NotMapped]
        public IUser Target { get; set; }
        [NotMapped]
        public IUser Moderator { get; set; }
        [NotMapped]
        public TimeSpan? Duration { get; set; }

        public override string ToString()
            => $"[{Id}] {Reason} • {((InfractionType) ModerationTypeId).Humanize()}{(Duration.HasValue ? $" • {Duration.Value.Humanize()}" : "")} • {Date.Humanize()}";
    }
}