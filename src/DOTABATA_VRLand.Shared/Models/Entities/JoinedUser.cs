using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DOTABATA_VRLand.Shared.Interfaces.StreamingHubs {
    /// <summary>
    /// 接続済みユーザー
    /// </summary>
    [MessagePackObject]
    public class JoinedUser {
        /// <summary>
        /// 接続id
        /// </summary>
        [Key(0)]
        public Guid ConnectionId { get; set; }
        /// <summary>
        /// 名前
        /// </summary>
        [Key(1)]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 参加順番
        /// </summary>
        [Key(2)] public int JoinOrder { get; set; }

        [Key(3)]
        public Color HeadColor = Color.white;
        [Key(4)]
        public string HatName = "None";
        [Key(5)]
        public string AccessoriesName = "None";
    }
}
