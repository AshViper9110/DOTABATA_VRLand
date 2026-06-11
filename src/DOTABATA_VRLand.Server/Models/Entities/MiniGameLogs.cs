using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOTABATA_VRLand.Server.Models.Entities
{
    [Table("miniGame_logs")]
    public class MiniGameLogs
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("miniGame_status_id")]
        public int MiniGameStatusId { get; set; }

        [Column("user_id")]
        public int Userid { get; set; }

        [Column("meinGame_log_id")]
        public int MeinGameLogId { get; set; }//タイムアタック:1 スコアアタック:2

        [Column("Score")]
        public int Score { get; set; }//勝利数

        [Column("user_rank")]
        public int UserRank { get; set; }//順位

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
