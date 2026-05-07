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

        //準備完了状態の変更
        public void UpdateReadyState(Guid connectionId, bool isReady)
        {
            // 対象ユーザーが存在しない場合は何もしない
            if (!RoomUserDataList.TryGetValue(connectionId, out var user))
            {
                Console.WriteLine($"[RoomContext]対象プレイヤーはルームに存在しません");
                return;
            }

            // Ready状態を更新
            user.IsReady = isReady;

            //コンソールに出力
            if(user.IsReady == true)
            {
                Console.WriteLine($"{user.joinedUser.Name}の準備が完了しました");
            }else
            {
                Console.WriteLine($"{user.joinedUser.Name}の準備完了が取り消されました");
            }
               
           
            if (IsAllUserReady() == true )
            {
                Console.WriteLine("すべてのプレイヤーの準備完了");
            }else
            {
                Console.WriteLine("すべてのプレイヤーの準備が完了していません");
            }

        }
    
        // 全員準備完了かどうかの判定処理
       public bool IsAllUserReady()
        {
            // 誰もいない場合は false
            if (RoomUserDataList.Count == 0)
            {
                Console.WriteLine("[RoomContext] IsAllUserReady: no users");
                return false;
            }

            // 1人でも Ready でなければ false
            foreach (var user in RoomUserDataList.Values)
            {
                if (!user.IsReady)
                {
                    Console.WriteLine(
                    $"[RoomContext] Not ready: Name={user.joinedUser.Name}"
                     );

                    return false;
                }
            }
            // 全員 Ready
            return true;
        }
    }
}
