using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using PeachtreeBus.Testing;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

public abstract class ProcessEstimatorFixtureBase<TEstimator> where TEstimator : IEstimator
{
    protected TEstimator _estimator = default!;
    protected readonly Mock<IBusDataAccess> _dataAccess = new();
    protected readonly Mock<IBusConfiguration> _busConfiguration = new();

    protected abstract void Given_Configuration();
    protected abstract void Given_NoConfiguration();
    protected abstract void Given_DataAccessResult(long result);
    protected abstract void Given_DataAccessThrows<T>(T exception) where T : System.Exception;

    protected abstract void VerifyDataAccessArguments();

    [TestMethod]
    [DataRow(true, 0, 0)]
    [DataRow(true, 1, 1)]
    [DataRow(true, 42, 42)]
    [DataRow(false, 0, 0)]
    [DataRow(false, 1, 0)]
    [DataRow(false, 42, 0)]
    public async Task Given_Configured_And_DataAcessResult_When_EstimateDemand_Then_ResultIsFromDataAccess(bool configured, int dataAccessResult, int expected)
    {
        if (configured) { Given_Configuration(); } else { Given_NoConfiguration(); }
        Given_DataAccessResult(dataAccessResult);
        var actual = await _estimator.EstimateDemand();
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task Given_Configuration_and_DataAccessWillThrow_When_EstimateDemand_Then_Throws()
    {
        Given_Configuration();
        var ex = new TestException();
        Given_DataAccessThrows(ex);
        var thrown = await Assert.ThrowsExactlyAsync<TestException>(_estimator.EstimateDemand);
        Assert.AreSame(ex, thrown);
    }

    [TestMethod]
    public async Task Given_Configuration_When_EstimateDemand_Then_DataAccessArgumentsAreCorrect()
    {
        Given_Configuration();
        await _estimator.EstimateDemand();
        VerifyDataAccessArguments();
    }
}
