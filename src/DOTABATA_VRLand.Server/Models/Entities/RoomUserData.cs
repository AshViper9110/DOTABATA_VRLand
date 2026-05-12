using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;

namespace DOTABATA_VRLand.Server.Models.Entities {
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
        public PlayerTransformDTO transform = new PlayerTransformDTO();

        /// <summary>
        /// ミニゲームの結果データ
        /// </summary>
        public MiniGameResultData miniGameResultData = new MiniGameResultData();

    }
}
