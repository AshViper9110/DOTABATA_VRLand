using DOTABATA_VRLand.Shared.Models.Entities;
using MagicOnion;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DOTABATA_VRLand.Shared.Interfaces.StreamingHubs {
    /// <summary>
    /// クライアントから呼び出す処理を実装するクラス用インターフェース
    /// </summary>
    public interface IRoomHub : IStreamingHub<IRoomHub, IRoomHubReceiver> {


        Task<List<MiniGameInfo>> GetAllMiniGameAsync();
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
        Task<Guid> CreateObjectAsync(SimpleTransform createdTransform, int minigameId, int objectListId);

        /// <summary>
        /// オブジェクトリストに追加
        /// </summary>
        Task AddObjectListAsync(Guid objectId, int minigameId, int objectListId, SimpleTransform simpleTransform);

        /// <summary>
        /// オブジェクトのTransform同期
        /// </summary>
        Task UpdateObjectTransformAsync(Guid objectId, SimpleTransform sTransform);

        /// <summary>
        /// オブジェクトの削除
        /// </summary>
        Task DestroyObjectAsync(Guid objectId, bool needOwnerShip);

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
        /// ミニゲームの結果を反映
        /// </summary>
        Task RegisterClearTimeAsync(DateTime time, bool firstWin);

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
        Task BallingNext(int pinCount, JoinedUser joinedUser);

        Task BallingPinAsync(int pinCount, JoinedUser joinedUser);


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
        Task SyncMagicBallAsync(Guid objectId, string gestureClassName, int rndNum);

        /// <summary>
        /// プレイヤーのステータス同期
        /// </summary>
        Task SyncPlayerStatusAsync(int hp);

        /// <summary>
        /// シールドのアクティブ状態同期
        /// </summary>
        Task ShieldActiveStateAsync(bool activeState);

        /// <summary>
        /// シャッターの開放同期
        /// </summary>
        Task OpenShutter(Guid connectionID);

        /// <summary>
        /// フリープレイのミニゲーム選択
        /// </summary>
       　Task SelectFreeMinigame(string name);

        /// <summary>
        /// ブロック崩しスコア送信
        /// </summary>
        Task BlockBreakSendScoreAsync(int score);

        /// <summary>
        /// 音の同期
        /// </summary>
        Task AudioAsync(int id);

        /// <summary>
        /// シーン移動
        /// </summary>
        Task MoveSceneAsync(string name);

<<<<<<< HEAD

        Task CutFood(Guid ID,Vector3 planePoint, Vector3 planeNormal);
=======
        /// <summary>
        /// スキン変更同期
        /// </summary>
        Task ChangeSkinAsync(Color headColor, string hatName, string accessoriesName);
>>>>>>> main
    }
}
