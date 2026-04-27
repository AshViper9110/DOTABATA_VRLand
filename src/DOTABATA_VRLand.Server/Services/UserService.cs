using DOTABATA_VRLand.Server.Models.Contexts;
using DOTABATA_VRLand.Shared.Interfaces.Services;
using MagicOnion.Server;

namespace DOTABATA_VRLand.Server.Services {
    public class UserService : ServiceBase<IUserService>, IUserService {
        //private readonly GameDbContext _context;

        //// DI
        //public UserService(GameDbContext context) {
        //    _context = context;
        //}
    }
}
