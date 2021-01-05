using System.Threading.Tasks;

namespace PeachtreeBus
{
    /// <summary>
    /// Defines an interface for code that runs an IRunOnIntervalTask.
    /// </summary>
    public interface IIntervalRunner
    {
        Task Run(IRunOnIntervalTask runOnIntervalTask);
    }
}
