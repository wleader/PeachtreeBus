using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface IEstimator
{
    Task<int> EstimateDemand();
}

public interface IAlwaysOneEstimator : IEstimator;

public class AlwaysOneEstimator : IAlwaysOneEstimator
{
    public Task<int> EstimateDemand() => Task.FromResult(1);
}
