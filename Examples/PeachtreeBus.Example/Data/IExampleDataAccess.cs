using Dapper;
using PeachtreeBus.DatabaseSharing;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Data
{
    /// <summary>
    /// An interface for the example application data access.
    /// </summary>
    public interface IExampleDataAccess
    {
        Task Audit(string message);
    }
}
