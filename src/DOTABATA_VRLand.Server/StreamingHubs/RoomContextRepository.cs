using Cysharp.Runtime.Multicast;
using DOTABATA_VRLand.Shared.Models.Entities;
using System.Collections.Concurrent;

namespace DOTABATA_VRLand.Server.StreamingHubs {
    public class RoomContextRepository(IMulticastGroupProvider groupProvider) {
        private readonly ConcurrentDictionary<Guid, RoomContext> contexts =
            new ConcurrentDictionary<Guid, RoomContext>();

        // ルームコンテキストの作成
        public RoomContext CreateContext(RoomConfig roomConfig) {
            var context = new RoomContext(groupProvider, roomConfig);
            contexts[context.Id] = context;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("{CreateRoom}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("<Room>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"Id：{context.Id}\n" +
                $"Name : {context.Name}\n"
                );

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
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{DeleteRoom}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("<Room>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"Id：{contexts[roomId].Id}\n" +
                $"Name : {contexts[roomId].Name}\n"
                );

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
