using Cysharp.Runtime.Multicast;
using System.Collections.Concurrent;

namespace DOTABATA_VRLand.Server.StreamingHubs {
    public class RoomContextRepository(IMulticastGroupProvider groupProvider) {
        private readonly ConcurrentDictionary<string, RoomContext> contexts =
            new ConcurrentDictionary<string, RoomContext>();

        // ルームコンテキストの作成
        public RoomContext CreateContext(string roomName) {
            var context = new RoomContext(groupProvider, roomName);
            contexts[roomName] = context;
            return context;
        }

        // ルームコンテキストの取得
        public RoomContext GetContext(string roomName) {
            if (!contexts.ContainsKey(roomName)) {
                return null;
            }
            return contexts[roomName];
        }

        // ルームコンテキストの削除
        public void RemoveContext(string roomName) {
            if (contexts.Remove(roomName, out var RoomContext)) {
                RoomContext?.Dispose();
            }
        }

        // 全ルームコンテキストの取得
        public ConcurrentDictionary<string, RoomContext> GetAllContext() {
            return contexts;
        }
    }
}
