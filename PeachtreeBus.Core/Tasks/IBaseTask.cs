using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface IBaseTask
{
    Task<bool> RunOne();
}
