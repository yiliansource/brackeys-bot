using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrackeysBot.Models.Database
{
    [Table("temporary_infractions")]
    public class TemporaryInfraction
    {
        [Key, Column("temp_infr_id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TemporaryInfractionId { get; set; }
        [Column("infractions_id")]
        public int InfractionId { get; set; }
        [Column("end_date")]
        public DateTime EndDate { get; set; }
    }
}