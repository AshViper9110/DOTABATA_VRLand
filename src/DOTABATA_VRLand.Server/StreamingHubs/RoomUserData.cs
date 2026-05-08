using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;

namespace DOTABATA_VRLand.Server.StreamingHubs {
    public class RoomUserData {
        /// <summary>
        /// 接続済みユーザー情報
        /// </summary>
        public JoinedUser joinedUser = new JoinedUser();

        /// <summary>
        /// ミニゲーム準備完了情報
        /// </summary>
        public bool IsReady;

        /// <summary>
        /// ユーザーのTransform情報
        /// </summary>
        public PlayerTransform transform = new PlayerTransform();

    }
}
