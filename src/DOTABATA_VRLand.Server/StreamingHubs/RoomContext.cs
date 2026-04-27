using Cysharp.Runtime.Multicast;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DOTABATA_VRLand.Server.StreamingHubs {
    public class RoomContext : IDisposable {
        public Guid Id { get; } // ルームid
        public string Name { get; } // ルーム名
        public IMulticastSyncGroup<Guid, IRoomHubReceiver> Group { get; } // グループ
        public Dictionary<Guid, RoomUserData> RoomUserDataList { get; } =
            new Dictionary<Guid, RoomUserData>(); // ユーザーデータ一覧

        // その他、ルームのデータとして保存したいものをフィールドに追加していく
        // コンストラクタ
        public RoomContext(IMulticastGroupProvider groupProvider, string roomName) {
            Id = Guid.NewGuid(); // ルーム毎のデータにIDを付けておく
            Name = roomName; // ルーム名をフィールドに保存
            Group = groupProvider.GetOrAddSynchronousGroup<Guid, IRoomHubReceiver>(roomName); // グループを作成
        }

        public void Dispose() {
            Group.Dispose();
        }
    }
}
