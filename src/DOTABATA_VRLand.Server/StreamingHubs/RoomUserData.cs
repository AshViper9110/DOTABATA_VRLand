using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;

namespace DOTABATA_VRLand.Server.StreamingHubs {
    public class RoomUserData {
        /// <summary>
        /// 接続済みユーザー情報
        /// </summary>
        public JoinedUser joinedUser = new JoinedUser();
        public bool IsReady;
    }
}
