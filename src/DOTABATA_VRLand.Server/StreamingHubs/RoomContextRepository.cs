using Cysharp.Runtime.Multicast;
using System.Collections.Concurrent;

namespace DOTABATA_VRLand.Server.StreamingHubs {
    public class RoomContextRepository(IMulticastGroupProvider groupProvider) {
        private readonly ConcurrentDictionary<Guid, RoomContext> contexts =
            new ConcurrentDictionary<Guid, RoomContext>();

        // ルームコンテキストの作成
        public RoomContext CreateContext(string roomName, string roomPassword) {
            var context = new RoomContext(groupProvider, roomName, roomPassword);
            contexts[context.Id] = context;
            return context;
        }

        // ルームコンテキストの取得（roomId）
        public RoomContext GetContext(Guid roomId) {
            if (!contexts.ContainsKey(roomId)) {
                return null;
            }
            return contexts[roomId];
        }

        // ルームコンテキストの取得（roomName）
        public RoomContext GetContext(string roomName) {
            if (!contexts.Any(_=>_.Value.Name == roomName)) {
                return null;
            }
            return contexts.FirstOrDefault(_=>_.Value.Name == roomName).Value;
        }

        // ルームコンテキストの削除（roomId）
        public void RemoveContext(Guid roomId) {
            if (contexts.Remove(roomId, out var RoomContext)) {
                RoomContext?.Dispose();
            }
        }

        // ルームコンテキストの削除（roomName）
        public void RemoveContext(string roomName) {
            if (!contexts.Any(_ => _.Value.Name == roomName)) {
                return;
            }

            RoomContext context  = contexts.FirstOrDefault(_ => _.Value.Name == roomName).Value;
            if (contexts.Remove(context.Id, out var RoomContext)) {
                RoomContext.Dispose();
            }
        }

        // 全ルームコンテキストの取得
        public ConcurrentDictionary<Guid, RoomContext> GetAllContext() {
            return contexts;
        }
    }
}
