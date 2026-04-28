using MessagePack;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOTABATA_VRLand.Server.Models.Entities {
    [Table("server_logs")]
    [MessagePackObject]
    public class ServerLogs {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public string Content {  get; set; }
    }
}
