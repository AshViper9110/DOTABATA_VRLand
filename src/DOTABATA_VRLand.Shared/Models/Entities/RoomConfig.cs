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
    [System.Serializable]
    public class RoomConfig {
        [Key(0)]
        public string Name { get; set; } = "";
        [Key(1)]
        public string Password { get; set; } = "";
        [Key(2)]
        public int GameModeId { get; set; } = -1;
    }
}
