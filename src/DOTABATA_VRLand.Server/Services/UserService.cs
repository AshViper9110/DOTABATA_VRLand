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
            AddServerLogs($"User search ID:All");
            await _context.SaveChangesAsync();
            return await _context.Users.ToArrayAsync();
        }

        /// <summary>
        /// ユーザー登録
        /// </summary>
        public async UnaryResult<bool> RegistUserAsync(string name) {
            await _semaphore.WaitAsync();

            try {
                // レコード追加
                User user = new User() {
                    Name = name,
                };
                _context.Users.Add(user);

                AddServerLogs($"Add user Name:{name}");

                await _context.SaveChangesAsync();

                return true;

            }catch (Exception e) {
                Console.WriteLine(e);
                return false;
            }
            finally {
                _semaphore.Release();
            }
        }
    }
}
