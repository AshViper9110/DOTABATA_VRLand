using DOTABATA_VRLand.Server.Models.Contexts;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using MagicOnion.Server.Hubs;
using Microsoft.EntityFrameworkCore;

namespace DOTABATA_VRLand.Server.StreamingHubs {
    public class RoomHub :StreamingHubBase<IRoomHub, IRoomHubReceiver>, IRoomHub {
        private readonly RoomContextRepository _roomContextRepository;
        //private readonly GameDbContext _dbContext;

        private RoomContext? _roomContext;

        public RoomHub(RoomContextRepository roomContextRepository) {
            _roomContextRepository = roomContextRepository;
        }

        //public RoomHub(RoomContextRepository roomContextRepository, GameDbContext dbContext) {
        //    _roomContextRepository = roomContextRepository;
        //    _dbContext = dbContext;
        //}

        /// <summary>
        /// 切断時の処理
        /// </summary>
        protected override ValueTask OnDisconnected() {
            // ルームから退出
            LeaveRoomAsync();

            return CompletedTask;
        }

        /// <summary>
        /// ルームに接続
        /// </summary>
        public Task<JoinedUser[]> JoinRoomAsync(string roomName, string userName) {
            // 同時に生成しない用に排他制御
            lock (_roomContextRepository) {
                // 指定の名前のルームがあるかどうかを確認
                this._roomContext = _roomContextRepository.GetContext(roomName);
                if (this._roomContext == null) {
                    // なかったら生成

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("{CreateRoom}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("RoomName : " + roomName + "\n");

                    this._roomContext = _roomContextRepository.CreateContext(roomName);
                }
            }
            // ルームに参加 ＆ ルームを保持
            this._roomContext.Group.Add(this.ConnectionId, Client);

            // 入室済みユーザーのデータを作成
            var joinedUser = new JoinedUser();
            joinedUser.ConnectionId = this.ConnectionId;
            joinedUser.Name = userName;
            joinedUser.JoinOrder = this._roomContext.RoomUserDataList.Count + 1;

            // ルームコンテキストにユーザー情報を登録
            var roomUserData = new RoomUserData() { joinedUser = joinedUser };
            this._roomContext.RoomUserDataList[this.ConnectionId] = roomUserData;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("{JoinRoom}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"RoomName : {roomName}\n" +
                $"Name : {roomUserData.joinedUser.Name}\n" +
                $"ConnectionID : {roomUserData.joinedUser.ConnectionId}\n" +
                $"JoinOrder : {roomUserData.joinedUser.JoinOrder}\n");


            // ルーム参加者全員に、ユーザーの入室通知を送信
            this._roomContext.Group.All.OnJoinRoom(joinedUser);

            // 入室リクエストをしたユーザーに、参加者の情報をリストで返す
            return Task.FromResult<JoinedUser[]> (this._roomContext.RoomUserDataList.Select(f => f.Value.joinedUser).ToArray());

        }

        /// <summary>
        /// 退出処理
        /// </summary>
        public Task LeaveRoomAsync() {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{LeaveRoom}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"RoomName : {_roomContext.Name}\n" +
                    $"Name : {_roomContext.RoomUserDataList[this.ConnectionId].joinedUser.Name}\n" +
                    $"ConnectionID : {this.ConnectionId}\n" +
                    $"JoinOrder : {_roomContext.RoomUserDataList[this.ConnectionId].joinedUser.JoinOrder}\n");

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

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{DeleteRoom}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("RoomName : " + _roomContext.Name + "\n");

                _roomContextRepository.RemoveContext(_roomContext.Name);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 接続ID取得
        /// </summary>
        public Task<Guid> GetConnectionId() {
            return Task.FromResult<Guid>(this.ConnectionId);
        }
    }
}
