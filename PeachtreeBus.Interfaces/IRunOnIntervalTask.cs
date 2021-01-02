using System.Threading.Tasks;

namespace PeachtreeBus
{
    public interface IRunOnIntervalTask
    {
        string Name { get; }
        Task Run();
        int SuccessWaitMs { get; }
        int ErrorWaitMs { get; }
    }
}
