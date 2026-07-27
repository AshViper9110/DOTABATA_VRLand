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

        public Dictionary<Guid, RoomObjectData> RoomObjectDataList { get; } =
            new Dictionary<Guid, RoomObjectData>(); // オブジェクトデータリスト


        public List<JoinedUser> GoalOrder = new List<JoinedUser>();
        private List<(JoinedUser user, int result)> rankOrderScore = new();
        private List<(JoinedUser user, DateTime time)> rankOrderTime = new();

        private int _currentCount = 3;//カウントダウン用

        public string Password { get; set; } // ルームパスワード

        // その他、ルームのデータとして保存したいものをフィールドに追加していく
        //ボーリングの順番
        public int ballingOrder;

        // ミニゲームのコンテスト
        public MiniGameContexts MiniGameContexts { get; set; } = new MiniGameContexts();

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
            rankOrderScore.Clear();//ミニゲーム開始時に毎回呼ぶ
            rankOrderTime.Clear();
        }

        /// <summary>
        /// ミニゲームの結果を反映
        /// </summary>
        public List<JoinedUser> ApplyMiniGameResultScore(Guid connectionId ,int result) {

            // 既にクリア済みの場合は無視
            if (rankOrderScore.Any(u => u.user.ConnectionId == connectionId))
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

            Console.WriteLine($"[RoomContext]GetScore{userData.joinedUser.Name}:{result}");

            //クリアした順番に追加
            rankOrderScore.Add((userData.joinedUser, result));

            //全員のデータがそろったタイミング
            if (rankOrderScore.Count == RoomUserDataList.Count)
            {
                // 順にソートして順位確定
                var ranked = rankOrderScore
                .OrderByDescending(u => u.result)
                .ThenBy(u => rankOrderScore.IndexOf(u)) // ゴールした順番を優先
                .Select(u => u.user)
                .ToList();

                //各プレイヤーの順位を保存
                for (int i = 0; i < ranked.Count; i++)
                {
                    if (!RoomUserDataList.TryGetValue(ranked[i].ConnectionId, out var roomUserData)) continue;

                    int rank = i + 1; // 0始まりなので+1
                    roomUserData.miniGameResultData.rankings.Add(rank); // 1位なら1, 2位なら2
                    //if (rank == 1) roomUserData.miniGameResultData.winCount++;//一位のプレイヤーは勝利カウントを+
             
                }



                return ranked;//joinedUser型の順位リストを返す
            }
            return null;
        }

        /// <summary>
        /// ミニゲームの結果を反映
        /// </summary>
        public List<JoinedUser> ApplyMiniGameResultTime(Guid connectionId, DateTime time, bool firstWin) {
            // 既にクリア済みの場合は無視
            if (rankOrderTime.Any(u => u.user.ConnectionId == connectionId)) {
                Console.WriteLine($"[RoomContext] クリアしているプレイヤーのクリア判定が行われました");
                return null;
            }

            // connectionIDを基にクリアユーザーの情報を取得
            if (!RoomUserDataList.TryGetValue(connectionId, out var userData)) {
                Console.WriteLine($"[RoomContext] クリアユーザーの情報の取得に失敗しました ID:{connectionId}");
                return null;
            }

            Console.WriteLine($"[RoomContext]GetTime{userData.joinedUser.Name}:{time}");
            //クリアした順番に追加
            rankOrderTime.Add((userData.joinedUser, time));

            //全員のデータがそろったタイミング
            if (rankOrderTime.Count == RoomUserDataList.Count) {
                List<JoinedUser> ranked;
                if (firstWin) {
                    // 順にソートして順位確定
                    ranked = rankOrderTime
                    .OrderBy(u => u.time)
                    .Select(u => u.user)
                    .ToList();
                }
                else {
                    // 順にソートして順位確定
                    ranked = rankOrderTime
                    .OrderByDescending(u => u.time)
                    .Select(u => u.user)
                    .ToList();
                }

                //各プレイヤーの順位を保存
                for (int i = 0; i < ranked.Count; i++) {
                    if (!RoomUserDataList.TryGetValue(ranked[i].ConnectionId, out var roomUserData)) continue;

                    int rank = i + 1; // 0始まりなので+1
                    roomUserData.miniGameResultData.rankings.Add(rank); // 1位なら1, 2位なら2
                                                                        //if (rank == 1) roomUserData.miniGameResultData.winCount++;//一位のプレイヤーは勝利カウントを+

                }



                return ranked;//joinedUser型の順位リストを返す
            }
            return null;
        }

        /// <summary>
        /// 全体の順位更新、送信
        /// </summary>
        public List<(JoinedUser user, int winCount)> SortAllRoundRanking()
        {
            foreach(var user in RoomUserDataList)
            {
                user.Value.IsReady = false;
            }


            var ranked = RoomUserDataList
                .OrderByDescending(u => u.Value.miniGameResultData.winCount)
                .ThenBy(u => u.Key)
                .Select(u => (u.Value.joinedUser, u.Value.miniGameResultData.winCount))
                .ToList();
            return ranked;
        }

        /// <summary>
        /// 準備完了状態の変更
        /// </summary>
        public List<(JoinedUser User, bool IsReady)> UpdateReadyState(Guid connectionId, bool isReady)
        {
            // 対象ユーザーが存在しない場合は何もしない
            if (!RoomUserDataList.TryGetValue(connectionId, out var user))
            {
                Console.WriteLine($"[RoomContext]対象プレイヤーはルームに存在しません");
                return null;
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

            // 全プレイヤーの準備状況をリストで返す
            return RoomUserDataList.Values
                .Select(u => (u.joinedUser, u.IsReady))
                .ToList();
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
        public int ResetCountdown(int count = 3)
        {
            _currentCount = count;
            return _currentCount;
        }

        /// <summary>
        /// プレイヤーの最終プレイ順位の取得
        /// </summary>
        public (JoinedUser? user, int ranking) GetLastMiniGameRanking(Guid connectionId)
        {
            // 対象ユーザーが存在しない場合は何もしない
            if (!RoomUserDataList.TryGetValue(connectionId, out var user))
            {
                Console.WriteLine($"[RoomContext]対象プレイヤーはルームに存在しません");
                return (null, -99); // ユーザーデータなし
            }

            var ranking = user.miniGameResultData.rankings.LastOrDefault(-1);
            return (user.joinedUser, ranking);

        }

        /// <summary>
        /// 勝利プレイヤーの勝利カウントUP
        /// </summary>
        public (JoinedUser? user, int winCount) WinCountUp(Guid connectionId)
        {
            // 対象ユーザーが存在しない場合は何もしない
            if (!RoomUserDataList.TryGetValue(connectionId, out var user))
            {
                Console.WriteLine($"[RoomContext]対象プレイヤーはルームに存在しません");
                return (null, -99); // ユーザーデータなし
            }

            user.miniGameResultData.winCount++;
            Console.WriteLine(""+user.miniGameResultData.winCount);
            return (user.joinedUser, user.miniGameResultData.winCount);

        }

        /// <summary>
        /// シーン移行状態変更
        /// 全員が完了したらTrue返す
        /// </summary>
        public bool ChangeIsCompleteSceneTransition(Guid connectionId) {
            // いるか
            if (!RoomUserDataList.Any(_=>_.Key == connectionId)) {
                return false;
            }

            RoomUserDataList[connectionId].IsCompleteSceneTransition = true;

            // 全員完了していたらfalseにもどしてTrue返す
            if (RoomUserDataList.Count(_=>_.Value.IsCompleteSceneTransition == true) == RoomUserDataList.Count()) {
                foreach (var user in RoomUserDataList.Values) {
                    user.IsCompleteSceneTransition = false;
                }

                return true;
            }
            
            return false;
        }
    }
}
