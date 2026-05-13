using Cysharp.Runtime.Multicast;
using DOTABATA_VRLand.Server.Models.Entities;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DOTABATA_VRLand.Server.StreamingHubs {
    public class RoomContext : IDisposable {
        public Guid Id { get; } // ルームid
        public string Name { get; } // ルーム名
        public IMulticastSyncGroup<Guid, IRoomHubReceiver> Group { get; } // グループ

        public int GameModeId { get; set; } // ゲームモードid
        public int MiniGameId { get; set; } // ミニゲームid

        public Dictionary<Guid, RoomUserData> RoomUserDataList { get; } =
            new Dictionary<Guid, RoomUserData>(); // ユーザーデータ一覧

        public List<JoinedUser> GoalOrder = new List<JoinedUser>();
        private int _currentCount = 3;//カウントダウン用

        public string Password { get; set; } // ルームパスワード

        // その他、ルームのデータとして保存したいものをフィールドに追加していく
        // コンストラクタ
        public RoomContext(IMulticastGroupProvider groupProvider, RoomConfig roomConfig) {
            Id = Guid.NewGuid(); // ルーム毎のデータにIDを付けておく
            Name = roomConfig.Name; // ルーム名をフィールドに保存
            Group = groupProvider.GetOrAddSynchronousGroup<Guid, IRoomHubReceiver>(roomConfig.Name); // グループを作成
            Password = roomConfig.Password;
            GameModeId = roomConfig.GameModeId;
        }

        public void Dispose() {
            Group.Dispose();
        }

        /// <summary>
        /// コンソールに入室ログを表示
        /// </summary>
        public void WriteConsoleJoinInfo(JoinedUser joinedUser) {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("{JoinRoom}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("<Room>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"Id：{Id}\n" +
                $"Name : {Name}"
                );

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("<User>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"Name : {joinedUser.Name}\n" +
                $"ConnectionID : {joinedUser.ConnectionId}\n" +
                $"JoinOrder : {joinedUser.JoinOrder}\n"
                );
        }

        /// <summary>
        /// コンソールに退室ログを表示
        /// </summary>
        public void WriteConsoleLeaveInfo(Guid connectionId) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{LeaveRoom}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("<Room>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"Id：{Id}\n" +
                $"Name : {Name}"
                );

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("<User>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"Name : {RoomUserDataList[connectionId].joinedUser.Name}\n" +
                $"ConnectionID : {connectionId}\n" +
                $"JoinOrder : {RoomUserDataList[connectionId].joinedUser.JoinOrder}\n"
                );
        }

        /// <summary>
        /// パスワードがあっているか
        /// </summary>
        public bool ComparePassword(string roomPassword) {
            return Password == roomPassword;
        }

        /// <summary>
        /// ミニゲームの結果を反映
        /// </summary>
        public void ApplyMiniGameResult(Dictionary<Guid, int> userRanks) {
            foreach (var user in userRanks) {
                MiniGameResultData miniGameResultData = RoomUserDataList[user.Key].miniGameResultData;

                miniGameResultData.rankings.Add(user.Value);
                miniGameResultData.point += user.Value;
                if (user.Value == 1) {
                    miniGameResultData.winCount++;
                }
            }

            SortAllRoundRanking();
        }

        /// <summary>
        /// 全体の順位更新
        /// </summary>
        public void SortAllRoundRanking() {
            int ranking = 1;
            foreach (var user in RoomUserDataList.OrderBy(_=>_.Value.miniGameResultData.winCount)) {
                user.Value.miniGameResultData.allRoundRanking = ranking;
                ranking++;
            }
        }

        /// <summary>
        /// 準備完了状態の変更
        /// </summary>
        public (JoinedUser user, bool isReady) UpdateReadyState(Guid connectionId, bool isReady)
        {
            // 対象ユーザーが存在しない場合は何もしない
            if (!RoomUserDataList.TryGetValue(connectionId, out var user))
            {
                Console.WriteLine($"[RoomContext]対象プレイヤーはルームに存在しません");
                return (null,false);
            }

            // Ready状態を更新
            user.IsReady = isReady;

            //コンソールに出力
            if(user.IsReady == true)
            {
                Console.WriteLine($"[RoomContext]{user.joinedUser.Name}の準備が完了しました");
            }else
            {
                Console.WriteLine($"[RoomContext]{user.joinedUser.Name}の準備完了が取り消されました");
            }
               
           
            if (IsAllUserReady() == true )
            {
                Console.WriteLine("[RoomContext]すべてのプレイヤーの準備完了");
            }else
            {
                Console.WriteLine("[RoomContext]すべてのプレイヤーの準備が完了していません");
            }

            return (user.joinedUser, user.IsReady);

        }

       
        /// <summary>
        /// 全員準備完了かどうかの判定処理
        /// </summary>
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

        /// <summary>
        /// カウントダウン
        /// </summary>
        public int TickCountdown()
        {
            if (_currentCount > 0)
            {
                _currentCount--;
            }
            return _currentCount;
        }

        /// <summary>
        /// カウントのリセット(未設定なら3で固定)
        /// </summary>
        public void ResetCountdown(int count = 3)
        {
            _currentCount = count;
        }

        /// <summary>
        /// 速度系順位確定
        /// </summary>
        public List<JoinedUser> RegisterGoal(Guid connectionId)
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
