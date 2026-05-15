using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOTABATA_VRLand.Shared.Models.Entities {
    /// <summary>
    /// ルームの設定
    /// </summary>
    [MessagePackObject]
    public class RoomInfo {
        [Key(0)]
        public string Name { get; set; } = "";
        [Key(1)]
        public bool UsePassword { get; set; }
        [Key(2)]
        public int GameModeId { get; set; } = -1;
        [Key(3)]
        public int PlayerAmount { get; set; } = -1;
    }
}
