using DOTABATA_VRLand.Shared.Models.Entities;

namespace DOTABATA_VRLand.Server.Models.Entities {
    public class RoomObjectData {
        /// <summary>
        /// オブジェクトリストId
        /// </summary>
        public int objectListId;

        /// <summary>
        /// Transform
        /// </summary>
        public SimpleTransform simpleTransform = new SimpleTransform();

        /// <summary>
        /// 所有者のConnectionId
        /// </summary>
        public Guid ownerConnectionId = Guid.Empty;
    }
}
