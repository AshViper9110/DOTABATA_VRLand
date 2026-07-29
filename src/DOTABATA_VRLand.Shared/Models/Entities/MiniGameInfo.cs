using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOTABATA_VRLand.Shared.Models.Entities
{
    [MessagePackObject]
    public class MiniGameInfo
    {
        [Key(0)]
        public byte[] BinaryImg { get; set; } = Array.Empty<byte>();
        [Key(1)]
        public string TitleName { get; set; }
        [Key(2)]
        public string SceneName { get; set; }
        [Key(3)]
        public bool IsPlayed { get; set; }
    }
}
