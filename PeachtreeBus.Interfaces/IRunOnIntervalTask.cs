using System.Threading.Tasks;

namespace PeachtreeBus
{
    /// <summary>
    /// Defines an interface for a task that wants to be run periodically.
    /// </summary>
    public interface IRunOnIntervalTask
    {
        /// <summary>
        /// A name for the task (for logging.)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The work to be done.
        /// </summary>
        /// <returns></returns>
        Task Run();

        /// <summary>
        /// How long to wait before running again when Run does not throw an exception.
        /// </summary>
        int SuccessWaitMs { get; }

        /// <summary>
        /// How long to wait before running again if Run did throw an exception.
        /// </summary>
        int ErrorWaitMs { get; }
    }
}
