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
        /// ルームを全取得
        /// </summary>
        Task<List<RoomInfo>> GetAllRoomAsync();

        /// <summary>
        /// ルームに接続
        /// </summary>
        Task<JoinedUser[]> JoinRoomAsync(string userName, RoomConfig roomConfig);

        /// <summary>
        /// 退出処理
        /// </summary>
        Task LeaveRoomAsync();

        /// <summary>
        /// 接続ID取得
        /// </summary>
        Task<Guid> GetConnectionId();


        /// <summary>
        /// ユーザーのTransfrom同期
        /// </summary>
        Task UpdateUserTransformAsync(PlayerTransformDTO playerTransform);


        /// <summary>
        /// ミニゲームの選択
        /// </summary>
        Task SelectMiniGameAsync(int miniGameId);

        /// <summary>
        /// ゲームスタート
        /// </summary>
        Task GameStartAsync();

        /// <summary>
        /// オブジェクト生成
        /// </summary>
        Task<Guid> CreateObjectAsync(SimpleTransform createdTransform, int objectListId);

        /// <summary>
        /// オブジェクトリストに追加
        /// </summary>
        Task AddObjectListAsync(Guid objectId, int objectListId, SimpleTransform simpleTransform);

        /// <summary>
        /// オブジェクトのTransform同期
        /// </summary>
        Task UpdateObjectTransformAsync(Guid objectId, SimpleTransform sTransform);

        /// <summary>
        /// オブジェクトの削除
        /// </summary>
        Task DestroyObjectAsync(Guid objectId);

        /// <summary>
        /// 準備完了切り替え
        /// </summary>
        Task UpdateReadyStateAsync(bool isReady);

        /// <summary>
        /// 3秒カウントダウン
        /// </summary>
        Task StartCountdownAsync();

        /// <summary>
        /// ミニゲーム結果処理
        /// </summary>
        Task RegisterScoreAsync(int result);

        /// <summary>
        /// ミニゲーム大会順位取得
        /// </summary>
        Task GetAllRoundRankingAsync();

        /// <summary>
        /// プレイヤー最終プレイ順位
        /// </summary>
        Task GetLastRankingAsync(Guid connectionId);
    }
}
