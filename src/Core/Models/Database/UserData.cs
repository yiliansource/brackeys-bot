using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrackeysBot.Models.Database
{
    public class UserData
    {
        [Key, Column("user_id"), Required]
        public ulong UserID { get; set;}
        [Column("stars"), Required]
        public int Stars { get; set; }
    }
}