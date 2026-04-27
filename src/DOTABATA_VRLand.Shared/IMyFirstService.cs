using MagicOnion;

namespace DOTABATA_VRLand.Shared
{
    public interface IMyFirstService : IService<IMyFirstService>
    {
        UnaryResult<int> SumAsync(int x, int y);
    }
}