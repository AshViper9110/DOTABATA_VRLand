using DOTABATA_VRLand.Server.Models.Contexts;
using DOTABATA_VRLand.Server.Models.Entities;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using MagicOnion.Server.Hubs;
using Microsoft.EntityFrameworkCore;

namespace DOTABATA_VRLand.Server.StreamingHubs {
    public class RoomHub :StreamingHubBase<IRoomHub, IRoomHubReceiver>, IRoomHub {
        private readonly RoomContextRepository _roomContextRepository;
        private readonly GameDbContext _dbContext;

        private RoomContext? _roomContext;

        //public RoomHub(RoomContextRepository roomContextRepository) {
        //    _roomContextRepository = roomContextRepository;
        //}

        public RoomHub(RoomContextRepository roomContextRepository, GameDbContext dbContext) {
            _roomContextRepository = roomContextRepository;
            _dbContext = dbContext;
        }

        /// <summary>
        /// 切断時の処理
        /// </summary>
        protected override ValueTask OnDisconnected() {
            // ルームから退出
            LeaveRoomAsync();

            return CompletedTask;
        }

        /// <summary>
        /// ルーム名を全取得
        /// </summary>
        public Task<List<string>> GetAllRoomNamesAsync(int gameModeId) {
            List<string> roomNames = new List<string>();
            if (gameModeId != -1)
            {
                foreach (var context in _roomContextRepository.GetAllContext())
                {
                    if (context.Value.GameModeId == gameModeId)
                    {
                        roomNames.Add(context.Value.Name);
                    }
                }
            }
            else
            {
                foreach (var context in _roomContextRepository.GetAllContext())
                {
                    roomNames.Add(context.Value.Name);
                }
            }

            return Task.FromResult<List<string>>(roomNames);
        }

        /// <summary>
        /// ルーム作成
        /// </summary>
        public Task CreateRoomAsync(RoomConfig roomConfig) {
            // 同時に生成しない用に排他制御
            lock (_roomContextRepository) {
                // 指定の名前のルームがあるかどうかを確認
                this._roomContext = _roomContextRepository.GetContext(roomConfig.Name);
                if (this._roomContext == null) {
                    // なかったら生成
                    this._roomContext = _roomContextRepository.CreateContext(roomConfig);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// ルーム削除
        /// </summary>
        public Task DeleteRoomAsync() {
            _roomContextRepository.RemoveContext(_roomContext.Id);

            return Task.CompletedTask;
        }

        /// <summary>
        /// ルームに接続
        /// </summary>
        public async Task<JoinedUser[]> JoinRoomAsync(string userName, RoomConfig roomConfig) {
            await CreateRoomAsync(roomConfig);

            // パスワード判定
            if (_roomContext.Password != "" &&
                !_roomContext.ComparePassword(roomConfig.Password)) {
                throw new Exception("パスワードがちがいます。");
            }

            // ルームに参加 ＆ ルームを保持
            this._roomContext.Group.Add(this.ConnectionId, Client);

            //// DBからユーザー情報取得
            //User user = await _dbContext.Users.FirstAsync(user => user.Name == userName);

            // 入室済みユーザーのデータを作成
            var joinedUser = new JoinedUser();
            joinedUser.ConnectionId = this.ConnectionId;
            joinedUser.Name = userName;
            joinedUser.JoinOrder = this._roomContext.RoomUserDataList.Count + 1;

            // ルームコンテキストにユーザー情報を登録
            var roomUserData = new RoomUserData() { joinedUser = joinedUser };
            this._roomContext.RoomUserDataList[this.ConnectionId] = roomUserData;

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
        public Task LeaveRoomAsync() {
            // コンソールにログを表示
            _roomContext.WriteConsoleLeaveInfo(this.ConnectionId);

            // 退出したことを全メンバーに通知
            int LeaveJoinOrder = _roomContext.RoomUserDataList[this.ConnectionId].joinedUser.JoinOrder;
            this._roomContext.Group.All.OnLeaveRoom(this.ConnectionId, LeaveJoinOrder);

            // ルーム内のメンバーから自分を削除
            this._roomContext.Group.Remove(this.ConnectionId);

            // 参加順番を繰り下げ
            foreach (RoomUserData roomUserData in _roomContext.RoomUserDataList.Values) {
                if (roomUserData.joinedUser.JoinOrder > LeaveJoinOrder) {
                    roomUserData.joinedUser.JoinOrder -= 1;
                }
            }

            // ルームデータから退出したユーザーを削除
            this._roomContext.RoomUserDataList.Remove(this.ConnectionId);

            // ルーム内にユーザーが一人もいなかったらルームを削除
            if (this._roomContext.RoomUserDataList.Count == 0) {
                DeleteRoomAsync();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 接続ID取得
        /// </summary>
        public Task<Guid> GetConnectionId() {
            return Task.FromResult<Guid>(this.ConnectionId);
        }


        /// <summary>
        /// ユーザーのTransfrom同期
        /// </summary>
        public Task UpdateUserTransformAsync(PlayerTransformDTO playerTransform) {
            // サーバーに保持
            _roomContext.RoomUserDataList[this.ConnectionId].transform = playerTransform;

            // 自分以外のユーザーに通知
            _roomContext.Group.Except([this.ConnectionId]).OnUpdateUserTransform(this.ConnectionId, playerTransform);

            return Task.CompletedTask;
        }


        /// <summary>
        /// ミニゲームの選択
        /// </summary>
        public Task SelectMiniGameAsync(int miniGameId) {
            // サーバーに保持
            _roomContext.MiniGameId = miniGameId;

            // 自分以外に通知
            _roomContext.Group.Except([this.ConnectionId]).OnSelectMiniGame(miniGameId);
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// 準備完了状態の変更
        /// </summary>
        public async Task UpdateReadyState(bool isReady)
        {
            var (updatedUser, isReadyResult) = _roomContext.UpdateReadyState(ConnectionId, isReady);

            if (updatedUser == null) return; // 対象ユーザーが存在しない場合

            //全員に更新されたプレイヤーと準備状態を通知
            // Group.All.OnUpdateReadyState(updatedUser, isReadyResult); Interface追加後に解除

            if (_roomContext.IsAllUserReady() == true)
            {
                Console.WriteLine("[RoomHub]すべてのプレイヤーの準備完了");
            }
            else
            {
                Console.WriteLine("[RoomHub]すべてのプレイヤーの準備が完了していません");
            }
        }

        /// <summary>
        /// カウントダウン
        /// </summary>
        public async Task StartCountdown()
        {
            _roomContext.ResetCountdown(3);//カウントのリセット

            //最初だけ3秒待機
            await Task.Delay(3000);

            while (true)
            {
                int count = _roomContext.TickCountdown();
                //_roomContext.Group.All.OnCountdown(count);//現在のカウントを通知、nterface追加後に解除

                if (count == 0) break;

                await Task.Delay(1000); // 1秒おく
            }
        }

        /// <summary>
        /// ゲームスタート
        /// </summary>
        public Task GameStartAsync() {
            // 全員に通知
            _roomContext.Group.All.OnGameStart();

            return Task.CompletedTask;
        }

        /// <summary>
        /// 速度系順位確定
        /// </summary>
        public void RegisterGoal()
        {
            var goalOrder = _roomContext.RegisterGoal(ConnectionId);

            if (goalOrder == null) return; // まだ全員ゴールしていない

            // 全員ゴール完了、順位確定
            // Group.All.OnRegisterGoal(goalOrder); Interface追加後に解除
        }

        /// <summary>
        /// オブジェクト作成
        /// </summary>
        public Task<Guid> CreateObjectAsync(SimpleTransform createdTransform, string objecName) {
            // id作成
            Guid objId = Guid.NewGuid();

            Console.WriteLine(objId.ToString());

            // 情報作成
            RoomObjectData roomObjectData = new RoomObjectData() {
                objectName = objecName,
                simpleTransform = createdTransform,
                ownerConnectionId = this.ConnectionId,
            };

            // サーバーに保持
            this._roomContext.RoomObjectDataList[objId] = roomObjectData;

            // 自分以外に通知
            this._roomContext.Group.Except([this.ConnectionId]).OnCreateObject(objId, this.ConnectionId, createdTransform, objecName);

            return Task.FromResult<Guid>(objId);
        }

        /// <summary>
        /// オブジェクトのTransform同期
        /// </summary>
        public Task UpdateObjectTransformAsync(Guid objectId, SimpleTransform sTransform) {
            // そのオブジェクトIdがあるか所有者のIdが一致しているか
            if (!this._roomContext.RoomObjectDataList.ContainsKey(objectId) ||
                this._roomContext.RoomObjectDataList[objectId].ownerConnectionId != this.ConnectionId) {
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
        public Task DestroyObjectAsync(Guid objectId) {
            // そのオブジェクトIdがあるか所有者のIdが一致しているか
            if (!this._roomContext.RoomObjectDataList.ContainsKey(objectId) ||
                this._roomContext.RoomObjectDataList[objectId].ownerConnectionId != this.ConnectionId) {
                return Task.CompletedTask;
            }

            // サーバーから削除
            this._roomContext.RoomObjectDataList.Remove(objectId);

            // 自分以外に通知
            this._roomContext.Group.Except([this.ConnectionId]).OnDestroyObject(objectId);

            return Task.CompletedTask;
        }
    }
}
