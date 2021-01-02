using System.Threading.Tasks;

namespace PeachtreeBus
{
    public interface IIntervalRunner
    {
        Task Run(IRunOnIntervalTask runOnIntervalTask);
    }
}
