using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOTABATA_VRLand.Server.Models.Entities
{

    [Table("daily_active_users")]
    public class DailyActiveUser
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("activity_date")]
        public DateTime ActivityDate { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

}
