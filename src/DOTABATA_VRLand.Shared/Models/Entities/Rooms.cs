using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOTABATA_VRLand.Shared.Models.Entities
{
 
        [Table("rooms")]
        public class Rooms
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Column("name")]
            public string Name { get; set; }

            [Column("pass")]
            public string Pass { get; set; }

            [Column("game_mode_id")]
            public int GameModeId { get; set; }

        }
    
}
