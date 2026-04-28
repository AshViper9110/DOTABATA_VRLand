using DOTABATA_VRLand.Shared.Models.Entities;
using MagicOnion;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DOTABATA_VRLand.Shared.Interfaces.StreamingHubs {
    /// <summary>
    /// クライアントから呼び出す処理を実装するクラス用インターフェース
    /// </summary>
    public interface IRoomHub : IStreamingHub<IRoomHub, IRoomHubReceiver> {
        /// <summary>
        /// ルームに接続
        /// </summary>
        public Task<JoinedUser[]> JoinRoomAsync(string roomName, string userName);

        /// <summary>
        /// 退出処理
        /// </summary>
        public Task LeaveRoomAsync();

        /// <summary>
        /// 接続ID取得
        /// </summary>
        Task<Guid> GetConnectionId();



        /// <summary>
        /// Transform同期
        /// </summary>
        Task UpdateUserTransformAsync(SimpleTransform simpleTransform);
    }
}
