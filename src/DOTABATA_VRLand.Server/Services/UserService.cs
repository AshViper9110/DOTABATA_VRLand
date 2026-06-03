using DOTABATA_VRLand.Server.Models.Contexts;
using DOTABATA_VRLand.Server.Models.Entities;
using DOTABATA_VRLand.Shared.Interfaces.Services;
using DOTABATA_VRLand.Shared.Models.Entities;
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
        public async UnaryResult<User> GetUserFromIdAsync(int id) {
            if (!_context.Users.Any(user=> user.Id == id)){
                throw new Exception();
            }

            AddServerLogs($"User Search ID:{id}");
            await _context.SaveChangesAsync();

            return await _context.Users.FirstAsync(user => user.Id == id);
        }


        /// <summary>
        /// ユーザー登録
        /// </summary>
        public async UnaryResult<bool> RegistUserAsync(string name, ulong steamId)
        {
            await _semaphore.WaitAsync();

            try
            {
                User? user = await _context.Users
                    .FirstOrDefaultAsync(u => u.SteamId == steamId);//steamIDからユーザー取得

                if (user == null)//// 未登録ユーザーの場合
                {
                    user = new User()
                    {
                        Name = name,
                        SteamId = steamId,
                    };

                    _context.Users.Add(user);

                    //AddServerLogs($"Add User SteamId:{steamId} Name:{name}");
                }
                else  // 既存ユーザーの場合は最新のSteam名で更新
                {
                    
                    user.Name = name;
                }

                await _context.SaveChangesAsync();//DBに保存

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            finally
            {
                _semaphore.Release();//同時実行対策
            }
        }
    }
}
