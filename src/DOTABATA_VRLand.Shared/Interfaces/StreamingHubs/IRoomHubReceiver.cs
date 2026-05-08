using DOTABATA_VRLand.Shared.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
