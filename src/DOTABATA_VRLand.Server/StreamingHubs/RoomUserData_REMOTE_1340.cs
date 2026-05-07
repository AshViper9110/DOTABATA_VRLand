using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;

namespace DOTABATA_VRLand.Server.StreamingHubs {
    public class RoomUserData {
        /// <summary>
        /// 接続済みユーザー情報
        /// </summary>
        public JoinedUser joinedUser = new JoinedUser();

        /// <summary>
        /// ユーザーのTransform情報
        /// </summary>
        public SimpleTransform transform = new SimpleTransform();
    }
}
