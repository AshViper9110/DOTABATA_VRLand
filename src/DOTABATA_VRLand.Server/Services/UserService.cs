using DOTABATA_VRLand.Server.Models.Contexts;
using DOTABATA_VRLand.Server.Models.Entities;
using DOTABATA_VRLand.Shared.Interfaces.Services;
using DOTABATA_VRLand.Shared.Models.Entities;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;

namespace DOTABATA_VRLand.Server.Services {
    public class UserService : ServiceBase<IUserService>, IUserService {
        private readonly GameDbContext _context;

        // 排他制御用
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        // DI
        public UserService(GameDbContext context) {
            _context = context;
        }

        /// <summary>
        /// DBにサーバーログを追加
        /// </summary>
        private void AddServerLogs(string content) {
            ServerLogs serverLogs = new ServerLogs() {
                Content = content,
            };
            _context.serverLogs.Add(serverLogs);
        }

        /// <summary>
        /// 全ユーザー情報取得
        /// </summary>
        public async UnaryResult<User[]> GetAllUsersAsync() {
            AddServerLogs($"User Search All");
            await _context.SaveChangesAsync();
            return await _context.Users.ToArrayAsync();
        }

        /// <summary>
        /// Idからユーザー情報取得
        /// </summary>
        /// <returns></returns>
        public async UnaryResult<User> GetUserFromIdAsync(ulong steamId)
        {

            var hash = HashSteamId(steamId);//ハッシュ

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.SteamId == hash);//ユーザー検索

            if (user is null)
            {
                throw new ReturnStatusException(StatusCode.NotFound, $"User not found. SteamId:{steamId}");
            }

            AddServerLogs($"User Search SteamId:{steamId}");

            return user;
        }

        /// <summary>
        /// ユーザー登録
        /// </summary>
        public async UnaryResult<bool> RegistUserAsync(string name, ulong steamId)
        {
           
            await _semaphore.WaitAsync();
            Console.WriteLine("RegistUserAsync Start");
            try
            {
                var hash = HashSteamId(steamId);///ハッシュ

                User? user = await _context.Users
                    .FirstOrDefaultAsync(u => u.SteamId == hash);//steamIDからユーザー取得

                if (user == null)//// 未登録ユーザーの場合
                {
                    user = new User()
                    {
                        Name = name,
                        SteamId = hash,
                    };

                    _context.Users.Add(user);

                    //AddServerLogs($"Add User SteamId:{steamId} Name:{name}");
                }
                else  // 既存ユーザーの場合は最新のSteam名で更新
                {                   
                    user.Name = name;
                }

                await _context.SaveChangesAsync();//DBに保存
               

                Console.WriteLine($"fnish:user:{user.Name}");
                Console.WriteLine($"ID:{user.Id}");
                return true;
              
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegistUserAsync Error:{e}");
                return false;
            }
            finally
            {
                _semaphore.Release();//同時実行対策
            }
        }

        //steamIDのハッシュ化
        private static string HashSteamId(ulong steamId)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(steamId.ToString());
            var hash = System.Security.Cryptography.SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
