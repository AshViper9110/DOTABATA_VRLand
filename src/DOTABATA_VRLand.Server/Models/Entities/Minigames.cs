using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOTABATA_VRLand.Server.Models.Entities
{
    [Table("miniGames")]
    public class Minigames
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("icon")]
        public byte[] Icon { get; set; }

        [Column("rule")]
        public string Rule { get; set; }

        [Column("type")]
        public int Type { get; set; }//タイムアタック:1 スコアアタック:2

        [Column("scene_name")]
        public string SceneName { get; set; }

        [Column("playable")]
        public int Playable { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
