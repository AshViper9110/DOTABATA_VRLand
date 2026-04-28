using DOTABATA_VRLand.Server.Models.Entities;
using DOTABATA_VRLand.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DOTABATA_VRLand.Server.Models.Contexts {
    public class GameDbContext :DbContext {
        public DbSet<User> Users { get; set; }
        public DbSet<ServerLogs> serverLogs { get; set; }

        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }
    }
}
