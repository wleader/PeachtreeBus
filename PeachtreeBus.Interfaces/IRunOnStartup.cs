using System.Threading.Tasks;

namespace PeachtreeBus
{

    /// <summary>
    /// Defines an interface for code that should be run before the bus starts.
    /// </summary>
    public interface IRunOnStartup
    {
        Task Run();
    }
}
