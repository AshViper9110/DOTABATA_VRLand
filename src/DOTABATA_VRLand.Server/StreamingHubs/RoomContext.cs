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

        private List<(JoinedUser user, int result)> rankOrder = new();

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
        /// ミニゲーム大会用リスト生成
        /// </summary>
        public void InitializeMiniGameResultData()
        {
            foreach (var user in RoomUserDataList.Values)
            {
                // 既存のMiniGameResultDataをリセットする
                user.miniGameResultData = new MiniGameResultData();
               
            }
        }

        /// <summary>
        /// ミニゲーム順位リスト初期化
        /// </summary>
        public void InitializeScoreOrder()
        {
            rankOrder.Clear();//ミニゲーム開始時に毎回呼ぶ
        }

        /// <summary>
        /// ミニゲームの結果を反映
        /// </summary>
        public List<JoinedUser> ApplyMiniGameResultScore(Guid connectionId ,int result) {

            // 既にクリア済みの場合は無視
            if (rankOrder.Any(u => u.user.ConnectionId == connectionId))
            {
                Console.WriteLine($"[RoomContext] クリアしているプレイヤーのクリア判定が行われました");
                return null;
            }

            // connectionIDを基にクリアユーザーの情報を取得
            if (!RoomUserDataList.TryGetValue(connectionId, out var userData))
            {
                Console.WriteLine($"[RoomContext] クリアユーザーの情報の取得に失敗しました ID:{connectionId}");
                return null;
            }

            //クリアした順番に追加
            rankOrder.Add((userData.joinedUser, result));

            //全員のデータがそろったタイミング
            if (rankOrder.Count == RoomUserDataList.Count)
            {
                // 残り時間の多い順にソートして順位確定
                var ranked = rankOrder
                    .OrderByDescending(u => u.result)
                    .Select(u => u.user)
                    .ToList();

                //各プレイヤーの順位を保存
                for (int i = 0; i < ranked.Count; i++)
                {
                    if (!RoomUserDataList.TryGetValue(ranked[i].ConnectionId, out var roomUserData)) continue;

                    int rank = i + 1; // 0始まりなので+1
                    roomUserData.miniGameResultData.rankings.Add(rank); // 1位なら1, 2位なら2
                    if (rank == 1) roomUserData.miniGameResultData.winCount++;//一位のプレイヤーは勝利カウントを+
             
                }

                return ranked;//joinedUser型の順位リストを返す
            }
            return null;
        }

        /// <summary>
        /// 全体の順位更新、送信
        /// </summary>
        public List<JoinedUser> SortAllRoundRanking() {

            // winCountの多い順にソートして順位確定
            var ranked = RoomUserDataList
                .OrderByDescending(u => u.Value.miniGameResultData.winCount)
                .Select(u => u.Value.joinedUser) //joinedUserを取得
                .ToList();

            return ranked;
        }

        /// <summary>
        /// 準備完了状態の変更
        /// </summary>
        public (JoinedUser user, bool readyState) UpdateReadyState(Guid connectionId, bool isReady)
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
        /// プレイヤーの最終プレイ順位の取得
        /// </summary>
        public int GetLastMiniGameRanking(Guid connectionId)
        {
            // 対象ユーザーが存在しない場合は何もしない
            if (!RoomUserDataList.TryGetValue(connectionId, out var user))
            {
                Console.WriteLine($"[RoomContext]対象プレイヤーはルームに存在しません");
                return -99; //ユーザーデータなし
            }

            //まだ何も登録されていない場合
            if (user.miniGameResultData.rankings.Count == 0)
            {
                Console.WriteLine($"[RoomContext] ランキングデータが存在しません");
                return -1;  //ランキングデータなし
            }

            return user.miniGameResultData.rankings.Last();

        }
    }
}
