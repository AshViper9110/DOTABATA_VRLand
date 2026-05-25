using DOTABATA_VRLand.Server.Models.Entities;
using DOTABATA_VRLand.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DOTABATA_VRLand.Server.Models.Contexts {
    public class GameDbContext :DbContext {
        public DbSet<User> Users { get; set; }//userクラス
        public DbSet<ServerLogs> serverLogs { get; set; }//サーバーログ、たぶん使わない
        public DbSet<DailyActiveUser> DailyActiveUsers { get; set; }//デイリーアクティブユーザー

        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }
    }
}
