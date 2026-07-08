using DOTABATA_VRLand.Server.Models.Entities;

namespace DOTABATA_VRLand.Server.StreamingHubs {
    public class ArcanaContext {
        public List<Guid> PlayerConIdList { get; set; } = new List<Guid>();

        public ArcanaContext(Dictionary<Guid, RoomUserData> roomUserDataList) {
            foreach (Guid conId in roomUserDataList.Keys) {
                PlayerConIdList.Add(conId);
            }
        }

        /// <summary>
        /// プレイヤーが死亡して一人になったら勝った人のConnectionId返す
        /// </summary>
        public Guid DeathPlayerAndIsGameSet(Guid connectionId) {
            PlayerConIdList.Remove(connectionId);

            if (PlayerConIdList.Count == 1) {
                return PlayerConIdList.First();
            }

            return Guid.Empty;
        }
    }
}
