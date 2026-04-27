using Microsoft.EntityFrameworkCore;

namespace DOTABATA_VRLand.Server.Models.Contexts {
    public class GameDbContext :DbContext {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) {

        }
    }
}
