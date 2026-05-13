using DOTABATA_VRLand.Shared.Models.Entities;

namespace DOTABATA_VRLand.Server.Models.Entities {
    public class RoomObjectData {
        /// <summary>
        /// オブジェクト名
        /// </summary>
        public string objectName = "";

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
