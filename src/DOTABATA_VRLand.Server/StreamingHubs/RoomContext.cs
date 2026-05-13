using Cysharp.Runtime.Multicast;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DOTABATA_VRLand.Server.StreamingHubs {
    public class RoomContext : IDisposable {
        public Guid Id { get; } // ルームid
        public string Name { get; } // ルーム名
        public IMulticastSyncGroup<Guid, IRoomHubReceiver> Group { get; } // グループ

        public int MiniGameId { get; set; } // ミニゲームid

        public Dictionary<Guid, RoomUserData> RoomUserDataList { get; } =
            new Dictionary<Guid, RoomUserData>(); // ユーザーデータ一覧

        public List<JoinedUser> GoalOrder = new List<JoinedUser>();

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
        public List<JoinedUser> RegisterGoal(Guid connectionId)//ConnectionIDでRoomUserDataから対象を探し、JoinedUser型のリストで返している
        {
            // 既にクリア済みの場合は無視
            if (GoalOrder.Any(u => u.ConnectionId == connectionId)) //GoalOrder.joinedUser.ConnectionIdを参照
            {
                Console.WriteLine($"[RoomContext] クリアしているプレイヤーのクリア判定が行われました");
                return null;
            }
            //connectionIDを基にjoinedUser.ConnectionIDでクリアユーザーの情報を取得
            RoomUserData userData = RoomUserDataList.Values.FirstOrDefault(u => u.joinedUser.ConnectionId == connectionId);
            if (userData == null)//nullチェック
            {
                Console.WriteLine($"[RoomContext] クリアユーザーの情報の取得に失敗しました ID:{connectionId}");
                return null;
            }
            //クリアした順番にidを追加
            GoalOrder.Add(userData.joinedUser);
            //クリアしていないプレイヤーが一人になった
            if (GoalOrder.Count == RoomUserDataList.Count - 1)
            {
                // GoalOrderに含まれていない最後の1人を探して追加 
                var lastPlayer = RoomUserDataList.Values
                    .FirstOrDefault(u => !GoalOrder.Contains(u.joinedUser)); //JoinedUser同士で比較
                                                                             //最後の一人のプレイヤーの情報を持っているか
                if (lastPlayer != null)
                {
                    GoalOrder.Add(lastPlayer.joinedUser);
                    Console.WriteLine($"[RoomContext] 最下位プレイヤー追加: {lastPlayer}");
                }

                int i = 1;//コンソール表記用カウント

                //コンソール順位表記
                foreach (var user in GoalOrder)
                {            
                    Console.WriteLine($"[RoomContext]　{i}位:{user.Name} ID:{user.ConnectionId}");
                    i++;
                }
                // 順位確定したリストを返す(joinedUser型)
                return GoalOrder;
            }
            return null;
        }
    }
}
