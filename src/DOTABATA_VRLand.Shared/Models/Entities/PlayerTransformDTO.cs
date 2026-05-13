using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOTABATA_VRLand.Shared.Models.Entities {
    /// <summary>
    /// プレイヤーの部位ごとの位置情報
    /// </summary>
    [MessagePackObject]
    public class PlayerTransformDTO {
        [Key(0)]
        public SimpleTransform Head = new SimpleTransform();
        [Key(1)]
        public SimpleTransform LeftHand = new SimpleTransform();
        [Key(2)]
        public SimpleTransform RightHand = new SimpleTransform();
        [Key(3)]
        public SimpleTransform Body = new SimpleTransform();
    }
}
