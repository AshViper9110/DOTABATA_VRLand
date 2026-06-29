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
        Task<JoinedUser[]> JoinRoomAsync(ulong? steamId, RoomConfig roomConfig);

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
        /// 所有権を取得する
        /// </summary>
        Task<bool> GetOwnershipAsync(Guid objectId, bool forcibly = false);

        /// <summary>
        /// 所有権を放棄する
        /// </summary>
        Task OwnershipAbandonmentAsync(Guid objectId);

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

        /// <summary>
        /// ゲーム大会の司会進行
        /// </summary>
        Task HostProgress();

        /// <summary>
        /// 勝利カウントUP
        /// </summary>
        Task WinCountUpAsync(Guid connectionId);

        ///<summary>
        ///ニットとポイントの更新
        ///</summary>
        Task UpdateNit(Guid connectionId,float point);



        /// <summary>
        /// シーン移行が完了したことを他プレイヤーに伝える
        /// </summary>
        Task CompleteSceneTransition();

        /// <summary>
        /// Roomオーナーがmainのゲームを開始させることを通知する
        /// </summary>
        Task RoomStart();

        /// <summary>
        /// ボーリングの順番変え
        /// </summary>
        Task BallingNext();


        /// <summary>
        /// 当たったことを通知
        /// </summary>
        Task HitDodgeBall(Guid connectionId);

        /// <summary>
        /// 被爆したことを通知
        /// </summary>
        Task HitBomber(Guid connectionId);

        /// <summary>
        /// アルカナスケッチの初期化
        /// </summary>
        Task ArcanaInitGameAsync();

        /// <summary>
        /// 死亡同期
        /// </summary>
        Task DeathAsync();


        /// <summary>
        /// 絵描き板の表示非表示同期
        /// </summary>
        Task SwitchDrawBoadActiveAsync(bool active);

        /// <summary>
        /// 魔法オブジェクトのフィールド同期
        /// </summary>
        Task SyncMagicBallAsync(Guid objectId, string gestureClassName);

        /// <summary>
        /// プレイヤーのステータス同期
        /// </summary>
<<<<<<< HEAD
        Task SyncPlayerStatusAsync(float hp);
        /// <summary>
        /// シャッターの開放同期
        /// </summary>
        Task OpenShutter(Guid connectionID);
=======
        Task SyncPlayerStatusAsync(int hp);
>>>>>>> main
    }
}
