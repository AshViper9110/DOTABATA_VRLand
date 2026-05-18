using DOTABATA_VRLand.Shared.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        public void OnCreateObject(Guid objectId, Guid createrConnectionId, SimpleTransform createdTransform, int objecListId);

        /// <summary>
        /// オブジェクトのTransform通知
        /// </summary>
        public void OnUpdateObjectTransform(Guid objectId, SimpleTransform sTransform);

        /// <summary>
        /// オブジェクトの削除通知
        /// </summary>
        public void OnDestroyObject(Guid objectId);

        /// <summary>
        /// 個人準備完了状態切り替え通知
        /// </summary>
        void OnUpdateReadyState(JoinedUser updatedUser, bool isReady);

        /// <summary>
        /// 全員準備完了状態切り替え通知
        /// </summary>
        void OnUpdateAllReadyState(bool isAllReady);

        /// <summary>
        /// カウントダウンカウント通知
        /// </summary>
        void OnCountdown(int count);

        /// <summary>
        /// ミニゲーム結果順位通知
        /// </summary>
        void OnRegisterScore(List<JoinedUser> rankOrder);

        /// <summary>
        /// ミニゲーム大会全体順位通知
        /// </summary>
        void OnGetAllRoundRanking(List<JoinedUser> ranking);

        /// <summary>
        /// プレイヤー最終プレイ順位通知
        /// </summary>
        void OnGetLastMiniGameRanking(Guid connectionId, int lastRank);
    }

}
