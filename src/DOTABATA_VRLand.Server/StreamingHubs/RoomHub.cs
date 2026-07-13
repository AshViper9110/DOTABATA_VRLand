using DOTABATA_VRLand.Server.Models.Contexts;
using DOTABATA_VRLand.Server.Models.Entities;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using MagicOnion.Server.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DOTABATA_VRLand.Server.StreamingHubs {
    public class RoomHub : StreamingHubBase<IRoomHub, IRoomHubReceiver>, IRoomHub
    {
        private readonly RoomContextRepository _roomContextRepository;
        private readonly GameDbContext _dbContext;                                                                  
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private RoomContext? _roomContext;

        /*
         * ミニゲーム用
         */

        private ArcanaContext? _arcanaContext;

        //public RoomHub(RoomContextRepository roomContextRepository) {
        //    _roomContextRepository = roomContextRepository;
        //}

        public RoomHub(RoomContextRepository roomContextRepository, GameDbContext dbContext, IServiceScopeFactory serviceScopeFactory)
        {
            _roomContextRepository = roomContextRepository;
            _dbContext = dbContext;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// 切断時の処理
        /// </summary>
        protected override ValueTask OnDisconnected()
        {
            // ルームから退出
            LeaveRoomAsync();

            return CompletedTask;
        }


        /// <summary>
        /// ルームを全取得
        /// </summary>
        public Task<List<RoomInfo>> GetAllRoomAsync() {
            List<RoomInfo> roomInfoList = new List<RoomInfo>();
            foreach (var context in _roomContextRepository.GetAllContext()) {
                RoomInfo roomInfo = new RoomInfo() {
                    Name = context.Value.Name,
                    UsePassword = context.Value.Password != "",
                    GameModeId = context.Value.GameModeId,
                    PlayerAmount = context.Value.RoomUserDataList.Count,
                };

                roomInfoList.Add(roomInfo);
            }

            return Task.FromResult<List<RoomInfo>>(roomInfoList);
        }

        /// <summary>
        /// ルーム作成
        /// </summary>
        public async Task CreateRoomAsync(RoomConfig roomConfig)
        {
            // 同時に生成しない用に排他制御
            lock (_roomContextRepository)
            {
                // 指定の名前のルームがあるかどうかを確認
                this._roomContext = _roomContextRepository.GetContext(roomConfig.Name);
                if (this._roomContext == null)
                {
                    // なかったら生成
                    this._roomContext = _roomContextRepository.CreateContext(roomConfig);
                    
                }
            }

            var exists = await _dbContext.Rooms
                     .AnyAsync(r => r.Name == roomConfig.Name);//DBにルームが存在するかチェック

            if (!exists)//新規
            {
                var room = new Rooms()
                {
                    Name = roomConfig.Name,
                    Pass = roomConfig.Password,
                    GameModeId = roomConfig.GameModeId
                };

                _dbContext.Rooms.Add(room);
                await _dbContext.SaveChangesAsync();//保存

                Console.WriteLine($"[DB] Room Created Id:{room.Id} Name:{room.Name}");
            }           
        }

        /// <summary>
        /// ルーム削除
        /// </summary>
        public　async Task DeleteRoomAsync()
        {
            _roomContextRepository.RemoveContext(_roomContext.Id);

            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();

            var room = await db.Rooms
                .FirstOrDefaultAsync(r => r.Name == _roomContext.Name);//データベーステーブル削除

            if (room != null)//削除できていれば
            {
                db.Rooms.Remove(room);
                await db.SaveChangesAsync();
                Console.WriteLine($"[DB] Room Deleted Id:{room.Id} Name:{room.Name}");
            }

        }

        /// <summary>
        /// ルームに接続
        /// </summary>
        public async Task<JoinedUser[]> JoinRoomAsync(ulong? steamId, RoomConfig roomConfig)
        {
            await CreateRoomAsync(roomConfig);

            // 4人以上いたら入室させない
            if (_roomContext.RoomUserDataList.Count >= 4) {
                throw new Exception("満室です。");
            }

            // パスワード判定
            if (_roomContext.Password != "" &&
                !_roomContext.ComparePassword(roomConfig.Password))
            {
                throw new Exception("パスワードがちがいます。");
            }

            // すでにいるか
            if (this._roomContext.RoomUserDataList.ContainsKey(this.ConnectionId)) {
                throw new Exception("すでに入室済みです。");
            }

            // ルームに参加 ＆ ルームを保持
            this._roomContext.Group.Add(this.ConnectionId, Client);

            var joinedUser = new JoinedUser();

            if(steamId != null)
            {
                var hash = HashSteamId(steamId.Value);///ハッシュ

                // DBからユーザー情報取得
                User user = await _dbContext.Users.FirstAsync(user => user.SteamId == hash);

                // 今日すでにアクティブ記録があるか確認
                var today = DateTime.Today;
                var existingRecord = await _dbContext.DailyActiveUsers
                    .FirstOrDefaultAsync(d => d.UserId == user.Id
                                            && d.ActivityDate.Date == today);

                if (existingRecord == null)
                {
                    // 今日初めてのログインなら登録
                    _dbContext.DailyActiveUsers.Add(new DailyActiveUser
                    {
                        UserId = user.Id,
                        ActivityDate = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                    await _dbContext.SaveChangesAsync();
                    Console.WriteLine($"[DB] {user.Name} の本日初回ログインを記録");
                }
                else
                {
                    Console.WriteLine($"[DB] {user.Name} は本日すでにログイン済み");
                }

                // 入室済みユーザーのデータを作成
                joinedUser.ConnectionId = this.ConnectionId;
                joinedUser.Name = user.Name;
                joinedUser.JoinOrder = this._roomContext.RoomUserDataList.Count + 1;

                // ルームコンテキストにユーザー情報を登録
                var roomUserData = new RoomUserData() { joinedUser = joinedUser, DbId = user.Id };
                this._roomContext.RoomUserDataList[this.ConnectionId] = roomUserData;

                // ★ ルームのDB IDを取得
                var room = await _dbContext.Rooms.FirstAsync(r => r.Name == _roomContext.Name);

                // RoomUser を登録
                _dbContext.RoomUsers.Add(new RoomUser
                {
                    RoomId = room.Id,
                    UserId = user.Id,
                });
                await _dbContext.SaveChangesAsync();

                Console.WriteLine($"[DB] Room Join Room:{room.Name} Name:{user.Name}");
            }else
            {
                User user = new User();
                user.Name = "Gest";

                // 入室済みユーザーのデータを作成
                joinedUser.ConnectionId = this.ConnectionId;
                joinedUser.Name = user.Name;
                joinedUser.JoinOrder = this._roomContext.RoomUserDataList.Count + 1;



                // ルームコンテキストにユーザー情報を登録
                var roomUserData = new RoomUserData() { joinedUser = joinedUser, DbId = user.Id };
                this._roomContext.RoomUserDataList[this.ConnectionId] = roomUserData;

            }

            // コンソールにログを表示
            _roomContext.WriteConsoleJoinInfo(joinedUser);

            // ルーム参加者全員に、ユーザーの入室通知を送信
            this._roomContext.Group.All.OnJoinRoom(joinedUser);

            // 入室リクエストをしたユーザーに、参加者の情報をリストで返す
            return this._roomContext.RoomUserDataList.Select(f => f.Value.joinedUser).ToArray();

        }

        /// <summary>
        /// 退出処理
        /// </summary>
        public async Task LeaveRoomAsync() {
            if (this._roomContext == null) return;
            // ルームにいなかったら無視
            if (!this._roomContext.RoomUserDataList.ContainsKey(this.ConnectionId)) {
                return;
            }

            // コンソールにログを表示
            _roomContext.WriteConsoleLeaveInfo(this.ConnectionId);

            // 退出したことを全メンバーに通知
            int LeaveJoinOrder = _roomContext.RoomUserDataList[this.ConnectionId].joinedUser.JoinOrder;
            this._roomContext.Group.All.OnLeaveRoom(this.ConnectionId, LeaveJoinOrder);

            // ルーム内のメンバーから自分を削除
            this._roomContext.Group.Remove(this.ConnectionId);

            // 参加順番を繰り下げ
            foreach (RoomUserData roomUserData in _roomContext.RoomUserDataList.Values)
            {
                if (roomUserData.joinedUser.JoinOrder > LeaveJoinOrder)
                {
                    roomUserData.joinedUser.JoinOrder -= 1;
                }
            }

            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();

            int userId = this._roomContext.RoomUserDataList[this.ConnectionId].DbId;
            var room = await db.Rooms.FirstAsync(r => r.Name == _roomContext.Name);
            var roomUser = await db.RoomUsers
                .FirstOrDefaultAsync(ru => ru.RoomId == room.Id && ru.UserId == userId);
            if (roomUser != null)
            {
                db.RoomUsers.Remove(roomUser);
                await db.SaveChangesAsync();
            }

            // ルームデータから退出したユーザーを削除
            this._roomContext.RoomUserDataList.Remove(this.ConnectionId);

            // ルーム内にユーザーが一人もいなかったらルームを削除
            if (this._roomContext.RoomUserDataList.Count == 0)
            {
                await DeleteRoomAsync();
            }

            //return Task.CompletedTask;
        }

        /// <summary>
        /// 接続ID取得
        /// </summary>
        public Task<Guid> GetConnectionId()
        {
            return Task.FromResult<Guid>(this.ConnectionId);
        }


        /// <summary>
        /// ユーザーのTransfrom同期
        /// </summary>
        public Task UpdateUserTransformAsync(PlayerTransformDTO playerTransform)
        {
            // サーバーに保持
            _roomContext.RoomUserDataList[this.ConnectionId].transform = playerTransform;

            // 自分以外のユーザーに通知
            _roomContext.Group.Except([this.ConnectionId]).OnUpdateUserTransform(this.ConnectionId, playerTransform);

            return Task.CompletedTask;
        }


        /// <summary>
        /// ミニゲームの選択
        /// </summary>
        public Task SelectMiniGameAsync(int miniGameId)
        {
            // サーバーに保持
            _roomContext.MiniGameId = miniGameId;

            // 自分以外に通知
            _roomContext.Group.Except([this.ConnectionId]).OnSelectMiniGame(miniGameId);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 準備完了状態の変更
        /// </summary>
        public async Task UpdateReadyStateAsync(bool isReady)
        {
            var allReadyStates =
                _roomContext.UpdateReadyState(ConnectionId, isReady);

            if (allReadyStates == null) return;

            var users =
                allReadyStates.Select(x => x.User).ToArray();

            var isReadyList =
                allReadyStates.Select(x => x.IsReady).ToArray();

            _roomContext.Group.All.OnUpdateReadyState(
                users,
                isReadyList);

            bool isAllReady = _roomContext.IsAllUserReady();

            Console.WriteLine(
                isAllReady
                    ? "[RoomHub]すべてのプレイヤーの準備完了"
                    : "[RoomHub]すべてのプレイヤーの準備が完了していません");

            _roomContext.Group.All.OnUpdateAllReadyState(
                isAllReady);
        }

        /// <summary>
        /// カウントダウン
        /// </summary>
        public async Task StartCountdownAsync()
        {
            if (!_roomContext.RoomUserDataList
                .TryGetValue(ConnectionId, out var self))
            {
                return;
            }

            int minOrder =
                _roomContext.RoomUserDataList.Values
                .Min(u => u.joinedUser.JoinOrder);

            if (self.joinedUser.JoinOrder != minOrder)
            {
                return;
            }

            int count = _roomContext.ResetCountdown(3);

            while (count > 0)
            {
                _roomContext.Group.All.OnCountdown(count);

                Console.WriteLine($"カウントダウン:{count}");

                await Task.Delay(1000);

                count = _roomContext.TickCountdown();
            }

            _roomContext.Group.All.OnCountdown(0);
        }

        /// <summary>
        /// ゲームスタート
        /// </summary>
        public Task GameStartAsync()
        {
            //ボーリングの順番リセット
            _roomContext.ballingOrder = 1;
            //ミニゲーム順位リストの初期化
            _roomContext.InitializeScoreOrder();
            // 全員に通知
            _roomContext.Group.All.OnGameStart();
            Console.WriteLine("[RoomHub]ゲーム開始");

            return Task.CompletedTask;
        }

        /// <summary>
        /// ミニゲームの結果を反映
        /// </summary>       
        /// <remarks>
        /// 制限時間の場合、Unity側でfloatをintに変換してから実行
        /// int remainingMs = (int)(remainingTime * 1000) でミリ秒に変換
        /// </remarks>
        public Task RegisterScoreAsync(int result)
        {

            var rankOrder = _roomContext.ApplyMiniGameResultScore(ConnectionId, result);

            if (rankOrder == null) return Task.CompletedTask;  // まだ全員ゴールしていない

            // 全員ゴール完了、順位確定
            _roomContext.Group.All.OnRegisterScore(rankOrder);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 全体の順位更新、送信
        /// </summary>
        public Task GetAllRoundRankingAsync()
        {

            var rank = _roomContext.SortAllRoundRanking();//順位リスト取得
            if (rank == null || rank.Count == 0) return Task.CompletedTask;

            var users = rank.Select(r => r.user).ToList();//ユーザーの順位順リストを取得
            var winCounts = rank.Select(r => r.winCount).ToList();//勝利数を取得、//    users[i] と winCounts[i] は必ず同じプレイヤーに対応

            // 呼び出した本人にだけ送信
            Client.OnGetAllRoundRanking(users, winCounts);
            return Task.CompletedTask;
            // 順位送信
           
        }

        /// <summary>
        /// プレイヤーの最終プレイ順位の取得
        /// </summary>
        public Task GetLastRankingAsync(Guid connectionId)
        {
            var (joinedUser, ranking) = _roomContext.GetLastMiniGameRanking(connectionId);

            if (joinedUser == null) return Task.CompletedTask;//nullチェック

            // 呼び出した本人にだけ送信
            Client.OnGetLastMiniGameRanking(joinedUser,ranking); 
            return Task.CompletedTask;
        }

        /// <summary>
        /// オブジェクト作成
        /// </summary>
        public Task<Guid> CreateObjectAsync(SimpleTransform createdTransform, int minigameId, int objectListId)
        {
            // id作成
            Guid objId = Guid.NewGuid();

            // 情報作成
            RoomObjectData roomObjectData = new RoomObjectData()
            {
                objectListId = objectListId,
                simpleTransform = createdTransform,
                ownerConnectionId = this.ConnectionId,
                ownerExist = true,
            };

            // サーバーに保持
            this._roomContext.RoomObjectDataList[objId] = roomObjectData;

            // 自分以外に通知
            this._roomContext.Group.Except([this.ConnectionId]).OnCreateObject(objId, this.ConnectionId, createdTransform, minigameId, objectListId);

            return Task.FromResult<Guid>(objId);
        }

        /// <summary>
        /// オブジェクトリストに追加
        /// </summary>
        public Task AddObjectListAsync(Guid objectId, int minigameId, int objectListId, SimpleTransform simpleTransform)
        {
            // 情報作成
            RoomObjectData roomObjectData = new RoomObjectData()
            {
                objectListId = objectListId,
                simpleTransform = simpleTransform,
                ownerConnectionId = this.ConnectionId,
            };

            // サーバーに保持
            this._roomContext.RoomObjectDataList[objectId] = roomObjectData;

            return Task.CompletedTask;
        }

        /// <summary>
        /// オブジェクトのTransform同期
        /// </summary>
        public Task UpdateObjectTransformAsync(Guid objectId, SimpleTransform sTransform)
        {
            // そのオブジェクトIdがあるか所有者のIdが一致しているか
            if (!this._roomContext.RoomObjectDataList.ContainsKey(objectId) ||
                this._roomContext.RoomObjectDataList[objectId].ownerConnectionId != this.ConnectionId)
            {
                return Task.CompletedTask;
            }

            // サーバーに保持
            this._roomContext.RoomObjectDataList[objectId].simpleTransform = sTransform;

            // 自分以外に通知
            this._roomContext.Group.Except([this.ConnectionId]).OnUpdateObjectTransform(objectId, sTransform);

            return Task.CompletedTask;
        }

        /// <summary>
        /// オブジェクトの削除
        /// </summary>
        public Task DestroyObjectAsync(Guid objectId, bool needOwnerShip) {
            // そのオブジェクトがあるか
            if (!this._roomContext.RoomObjectDataList.ContainsKey(objectId)) return Task.CompletedTask;

            // そのオブジェクトの所有者か
            if (needOwnerShip &&
                this._roomContext.RoomObjectDataList[objectId].ownerConnectionId != this.ConnectionId) return Task.CompletedTask;

            // サーバーから削除
            this._roomContext.RoomObjectDataList.Remove(objectId);

            // 自分以外に通知
            this._roomContext.Group.Except([this.ConnectionId]).OnDestroyObject(objectId);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 所有権を取得する
        /// </summary>
        public Task<bool> GetOwnershipAsync(Guid objectId, bool forcibly = false) {
            // そのプレイヤーとオブジェとが存在するか
            if (!this._roomContext.RoomUserDataList.ContainsKey(this.ConnectionId) ||
                !this._roomContext.RoomObjectDataList.ContainsKey(objectId)) {
                return Task.FromResult<bool>(false);
            }

            // もし所有者だったら何もしない
            if (this._roomContext.RoomObjectDataList[objectId].ownerConnectionId == this.ConnectionId) {
                return Task.FromResult<bool>(true);
            }

            // 前の所有者
            Guid beforeOwner = this._roomContext.RoomObjectDataList[objectId].ownerConnectionId;

            // 同時に所有権を取得しないように排他制御
            lock (this._roomContext.RoomObjectDataList) {
                // 強制じゃなければ
                if (!forcibly) {
                    // 別のプレイヤーが所有者を有していたら無効
                    if (this._roomContext.RoomObjectDataList[objectId].ownerExist) {
                        return Task.FromResult<bool>(false);
                    }
                }

                this._roomContext.RoomObjectDataList[objectId].ownerExist = true;
                this._roomContext.RoomObjectDataList[objectId].ownerConnectionId = this.ConnectionId;

                // 前の所有者に所有権削除通知をおくる
                this._roomContext.Group.Only([beforeOwner]).OnDeleateOwnership(objectId);
            }

            return Task.FromResult<bool>(true);
        }

        /// <summary>
        /// 所有権を放棄する
        /// </summary>
        public Task OwnershipAbandonmentAsync(Guid objectId) {
            if (this._roomContext == null) {
                return Task.CompletedTask;
            }

            // そのプレイヤーとオブジェとが存在するか
            if (!this._roomContext.RoomUserDataList.ContainsKey(this.ConnectionId) ||
                !this._roomContext.RoomObjectDataList.ContainsKey(objectId)) {
                return Task.CompletedTask;
            }

            // もし所有者じゃなかったら何もしない
            if (this._roomContext.RoomObjectDataList[objectId].ownerConnectionId != this.ConnectionId) {
                return Task.CompletedTask;
            }

            // 解除
            this._roomContext.RoomObjectDataList[objectId].ownerExist = false;

            return Task.CompletedTask;
        }
        /// <summary>
        /// 勝利プレイヤーの勝利カウントUP
        /// </summary>
        public Task WinCountUpAsync(Guid connectionId)
        {
            var (user, winCount) = _roomContext.WinCountUp(connectionId);

            if (user == null) return Task.CompletedTask; // ユーザーなし

            // 全員に通知
            _roomContext.Group.All.OnWinCountUp(user, winCount);
            return Task.CompletedTask;
        }


        /// <summary>
        /// ゲーム大会の司会進行
        /// </summary>
        public Task HostProgress()
        {
            // 全員（自分も含む）に通知
            this._roomContext.Group.All.OnHostProgress();
            return Task.CompletedTask;
        }

        ///<summary>
        ///ニットの更新
        /// </summary>
        public Task UpdateNit(Guid connectionId,float point)
        {
           
            // 全員（自分も含む）に通知
            this._roomContext.Group.All.OnUpdateNit(connectionId,point);
            return Task.CompletedTask;
        }

        /// <summary>
        /// シーン移行が完了したことを他プレイヤーに伝える
        /// </summary>
        public Task CompleteSceneTransition() {
            bool allComplete = this._roomContext.ChangeIsCompleteSceneTransition(this.ConnectionId);

            // 自分以外に完了通知
            this._roomContext.Group.Except([this.ConnectionId]).OnCompleteSceneTransition(this.ConnectionId);
            // もし全員が完了してたら
            if (allComplete) {
                // 全員に通知
                this._roomContext.Group.All.OnAllCompleteSceneTransition();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// アルカナスケッチの初期化
        /// </summary>
        public Task ArcanaInitGameAsync() {
            this._roomContext.MiniGameContexts._arcanaContext = new ArcanaContext(this._roomContext.RoomUserDataList);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 死亡同期
        /// </summary>
        public Task DeathAsync() {
            // 自分以外に通知
            this._roomContext.Group.Except([this.ConnectionId]).OnDeath(this.ConnectionId);

            lock (this._roomContext.MiniGameContexts._arcanaContext) {
                // コンテストからプレイヤーを削除
                Guid resultConId = this._roomContext.MiniGameContexts._arcanaContext.DeathPlayerAndIsGameSet(this.ConnectionId);
                // 一人になったら
                if (resultConId != Guid.Empty) {
                    // ゲーム終了と勝者のIdを全員に通知
                    this._roomContext.Group.All.OnArcanaGameSet(resultConId);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 絵描き板の表示非表示同期
        /// </summary>
        public Task SwitchDrawBoadActiveAsync(bool active) {
            // 自分以外に通知
            this._roomContext.Group.Except([this.ConnectionId]).OnSwitchDrawBoadActive(this.ConnectionId, active);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 魔法オブジェクトのフィールド同期
        /// </summary>
        public Task SyncMagicBallAsync(Guid objectId, string gestureClassName, int rndNum) {
            // 自分以外に通知
            this._roomContext.Group.Except([this.ConnectionId]).OnSyncMagicBall(objectId, this.ConnectionId, gestureClassName, rndNum);

            return Task.CompletedTask;
        }

        /// <summary>
        /// シールドのアクティブ状態同期
        /// </summary>
        public Task ShieldActiveStateAsync(bool activeState) {
            // 自分以外に通知
            this._roomContext.Group.Except([this.ConnectionId]).OnShieldActiveState(this.ConnectionId, activeState);

            return Task.CompletedTask;
        }

        //steamIDのハッシュ化
        private static string HashSteamId(ulong steamId)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(steamId.ToString());
            var hash = System.Security.Cryptography.SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }

        //ルームの開始
        public Task RoomStart()
        {
            this._roomContext.Group.All.OnRoomStart();
            return Task.CompletedTask;
        }

        //ボーリングの順番変え
        public Task BallingNext(int pinCount, JoinedUser joinedUser)
        {
            this._roomContext.ballingOrder++;
            this._roomContext.Group.All.OnBallingNext(this._roomContext.ballingOrder, joinedUser, pinCount);
            return Task.CompletedTask;
        }

        //爆弾ドッチボールのヒット処理
        public Task HitDodgeBall(Guid connectionId)
        {
            this._roomContext.Group.All.OnHitDodgeBall(connectionId);
            return Task.CompletedTask;
        }

        //爆弾ドッチボールの死亡処理
        public Task HitBomber(Guid connectionId) {
            this._roomContext.Group.All.OnHitBomber(connectionId);
            return Task.CompletedTask;
        }

        /// <summary>
        /// プレイヤーのステータス同期
        /// </summary>
        public Task SyncPlayerStatusAsync(int hp) {
            // 自分以外に通知
            this._roomContext.Group.Except([this.ConnectionId]).OnSyncPlayerStatus(this.ConnectionId, hp);

            return Task.CompletedTask;
        }


        /// <summary>
        /// シャッターの開放同期
        /// </summary>
        public Task OpenShutter(Guid connectionID)
        {
            // 全員に通知
            this._roomContext.Group.All.OnOpenShutter(connectionID);

            return Task.CompletedTask;
        }

        ///<summary>
        ///フリープレイのミニゲーム選択
        /// </summary>
        public Task SelectFreeMinigame(string name)
        {
            // 全員に通知
            this._roomContext.Group.All.OnSelectFreeMinigame(name);
            return Task.CompletedTask;
        }

        /// <summary>
        /// ブロック崩しスコア送信
        /// </summary>
        public Task BlockBreakSendScoreAsync(int score) {
            // 全員に通知
            this._roomContext.Group.All.OnBlockBreakSendScore(this.ConnectionId, score);
            return Task.CompletedTask;
        }
    }
}
