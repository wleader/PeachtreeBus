using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using System;

namespace PeachtreeBus.Core.Tests.Data;

public abstract class DapperDataAccess_FixtureBase
{
    protected DapperDataAccess _dataAccess = default!;
    private readonly Mock<IBusConfiguration> _busConfiguration = new();
    private readonly Mock<IDapperTypesHandler> _dapperTypes = new();
    private readonly Mock<IDapperMethods> _dapperMethods = new();
    protected readonly Mock<ISharedDatabase> _sharedDb = new();

    [TestInitialize]
    public void Initialize()
    {
        _dapperTypes.Reset();
        _busConfiguration.Reset();
        _dapperMethods.Reset();
        _sharedDb.Reset();

        _dapperTypes.Setup(t => t.Configure())
            .Returns(true);

        _busConfiguration.SetupGet(c => c.Schema)
            .Returns(new SchemaName("PBus"));

        _dataAccess = new(
            _sharedDb.Object,
            _busConfiguration.Object,
            FakeLog.Create<DapperDataAccess>(),
            _dapperMethods.Object);
    }

    protected void AssertStatementContains(string expected)
    {
        Assert.AreEqual(1, _dapperMethods.Invocations.Count);
        var statement = (string)_dapperMethods.Invocations[0].Arguments[0];
        Assert.IsTrue(statement.Contains(expected),
            $"The SQL Statement did not contain the expected fragment \"{expected}\".\r\nStatement:\r\n{statement}");
    }

    protected void AssertParameterSet<T>(string parameterName, T expected)
    {
        Assert.AreEqual(1, _dapperMethods.Invocations.Count);
        var parameters = (DynamicParameters?)_dapperMethods.Invocations[0].Arguments[1];
        Assert.IsNotNull(parameters);
        var actual = parameters.Get<T>(parameterName);
        Assert.AreEqual(expected, actual);
    }

    protected class TestException : Exception;

    protected void Given_DapperThrows<T>()
    {
        _dapperMethods.Setup(m => m.QueryFirst<T>(It.IsAny<string>(), It.IsAny<DynamicParameters?>()))
            .ThrowsAsync(new TestException());
        _dapperMethods.Setup(m => m.Query<T>(It.IsAny<string>(), It.IsAny<DynamicParameters?>()))
            .ThrowsAsync(new TestException());
        _dapperMethods.Setup(m => m.QueryFirstOrDefault<T>(It.IsAny<string>(), It.IsAny<DynamicParameters?>()))
            .ThrowsAsync(new TestException());
        _dapperMethods.Setup(m => m.ExecuteScalar<T>(It.IsAny<string>(), It.IsAny<DynamicParameters?>()))
            .ThrowsAsync(new TestException());
        _dapperMethods.Setup(m => m.Execute(It.IsAny<string>(), It.IsAny<DynamicParameters?>()))
            .ThrowsAsync(new TestException());
    }
}
