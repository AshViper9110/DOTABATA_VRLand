using DOTABATA_VRLand.Shared.Models.Entities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DOTABATA_VRLand.Shared.Interfaces.StreamingHubs {
    /// <summary>
    /// サーバーからクライアントへの通知関連
    /// </summary>
    public interface IRoomHubReceiver {
        /// <summary>
        /// ユーザーの入室通知
        /// </summary>
        public void OnJoinRoom(JoinedUser user);

        /// <summary>
        /// ユーザーの退室通知
        /// </summary>
        public void OnLeaveRoom(Guid connectionId, int joinOrder);


        /// <summary>
        /// ユーザーのTransfrom通知
        /// </summary>
        public void OnUpdateUserTransform(Guid connectionId, PlayerTransformDTO playerTransform);

        /// <summary>
        /// ミニゲームの選択通知
        /// </summary>
        public void OnSelectMiniGame(int miniGameId);

        /// <summary>
        /// ゲームスタート通知
        /// </summary>
        public void OnGameStart();

        /// <summary>
        /// オブジェクト作成通知
        /// </summary>
        public void OnCreateObject(Guid objectId, Guid createrConnectionId, SimpleTransform createdTransform, int minigameId, int objecListId);

        /// <summary>
        /// オブジェクトのTransform通知
        /// </summary>
        public void OnUpdateObjectTransform(Guid objectId, SimpleTransform sTransform);

        /// <summary>
        /// オブジェクトの削除通知
        /// </summary>
        public void OnDestroyObject(Guid objectId);

        /// <summary>
        /// 所有者削除通知
        /// </summary>
        public void OnDeleateOwnership(Guid objectId);

        /// <summary>
        /// 個人準備完了状態切り替え通知
        /// </summary>
        public void OnUpdateReadyState(JoinedUser[] users, bool[] isReadyList);

        /// <summary>
        /// 全員準備完了状態切り替え通知
        /// </summary>
        public void OnUpdateAllReadyState(bool isAllReady);

        /// <summary>
        /// カウントダウンカウント通知
        /// </summary>
        public void OnCountdown(int count);

        /// <summary>
        /// ミニゲーム結果順位通知
        /// </summary>
        public void OnRegisterScore(List<JoinedUser> rankOrder);

        /// <summary>
        /// ミニゲーム大会全体順位通知
        /// </summary>
        public void OnGetAllRoundRanking(List<JoinedUser> ranking, List<int> winCounts);

        /// <summary>
        /// プレイヤー最終プレイ順位通知
        /// </summary>
        public void OnGetLastMiniGameRanking(JoinedUser joinedUser, int lastRank);

        /// <summary>
        /// 勝利カウントUP通知
        /// </summary>
        public void OnWinCountUp(JoinedUser user, int winCount);


        /// <summary>
        /// ゲーム大会の司会進行
        /// </summary>
        void OnHostProgress();

        /// <summary>
        /// シーン移行完了通知
        /// </summary>
        public void OnCompleteSceneTransition(Guid connectionId);

        /// <summary>
        /// 全員のシーン移行完了通知
        /// </summary>
        public void OnAllCompleteSceneTransition();


        /// <summary>
        /// ニットの更新
        /// </summary>
        public void OnUpdateNit(Guid id,float point);

        /// <summary>
        /// mainのゲームの開始通知
        /// </summary>
        public void OnRoomStart();

        /// <summary>
        /// ボーリングの順番変え通知
        /// </summary>
        public void OnBallingNext(int order, JoinedUser joinedUser, int pinCount);

        public void OnBallingPinAsync(int pinCount, JoinedUser joinedUser);

        /// <summary>
        /// 爆弾ドッチボールのヒット通知
        /// </summary>
        public void OnHitDodgeBall(Guid connectionId);

        /// <summary>
        /// 爆弾ドッチボールの死亡通知
        /// </summary>
        public void OnHitBomber(Guid connectionId);

        /// <summary>
        /// 死亡通知
        /// </summary>
        public void OnDeath(Guid connectionId);

        /// <summary>
        /// アルカナスケッチのゲーム終了通知
        /// </summary>
        public void OnArcanaGameSet(Guid winnerConId);

        /// <summary>
        /// 絵描き板の表示非表示同期通知
        /// </summary>
        public void OnSwitchDrawBoadActive(Guid playerConId, bool active);

        /// <summary>
        /// 魔法オブジェクトのフィールド同期
        /// </summary>
        public void OnSyncMagicBall(Guid objectId, Guid createrConId, string gestureClassName, int rndNum);

        /// <summary>
        /// プレイヤーのステータス同期通知
        /// </summary>
        public void OnSyncPlayerStatus(Guid playerConId, int hp);

        /// <summary>
        /// シールドのアクティブ状態同期
        /// </summary>
        public void OnShieldActiveState(Guid playerConId, bool activeState);

        /// <summary>
        /// シャッターの開放通知
        /// </summary>
        public void OnOpenShutter(Guid connectionID);


        public void OnSelectFreeMinigame(string name);

        /// <summary>
        /// スコア送信通知
        /// </summary>
        public void OnBlockBreakSendScore(Guid playerConId, int score);

        /// <summary>
        /// 音の同期通知
        /// </summary>
        public void OnAudioAsync(int id);

        /// <summary>
        /// シーン移動
        /// </summary>
        public void OnMoveSceneAsync(string name);

        /// <summary>
<<<<<<< HEAD
        /// 食材のカット
        /// </summary>
        public void OnCutFood(Guid ID,UnityEngine.Vector3 planePoint, UnityEngine.Vector3 planeNormal);
=======
        /// スキン変更通知
        /// </summary>
        public void OnChangeSkin(Guid playerConId, Color headColor, string hatName, string accessoriesName);
>>>>>>> main
    }

}
