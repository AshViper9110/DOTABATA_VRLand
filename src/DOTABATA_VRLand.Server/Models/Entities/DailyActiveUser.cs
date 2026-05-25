using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOTABATA_VRLand.Server.Models.Entities
{

    [Table("daily_active_users")]
    public class DailyActiveUser
    {
        [Key]
        public int Id { get; set; }
        public DateTime ActivityDate { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDay { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
