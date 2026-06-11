using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOTABATA_VRLand.Server.Models.Entities
{
    [Table("room_users")]
    public class RoomUser
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("room_id")]
        public int RoomId { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
    }
}
