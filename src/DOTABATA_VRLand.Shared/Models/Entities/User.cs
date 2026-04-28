using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOTABATA_VRLand.Shared.Models.Entities {
    [Table("users")]
    [MessagePackObject]
    public class User {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public string Name { get; set; }
    }
}
