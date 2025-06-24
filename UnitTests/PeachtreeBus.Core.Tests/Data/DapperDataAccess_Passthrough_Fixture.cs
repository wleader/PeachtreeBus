using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_Passthrough_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public void Given_SharedDbWillThrow_When_BeginTransaction_Then_Throws()
    {
        _sharedDb.Setup(d => d.BeginTransaction()).Throws<TestException>();
        Assert.ThrowsExactly<TestException>(_dataAccess.BeginTransaction);
    }

    [TestMethod]
    public void When_BeginTransaction_Then_PassThrough()
    {
        _dataAccess.BeginTransaction();
        _sharedDb.Verify(d => d.BeginTransaction(), Times.Once);
        _sharedDb.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Given_SharedDbWillThrow_When_Reconnect_Then_Throws()
    {
        _sharedDb.Setup(d => d.Reconnect()).Throws<TestException>();
        Assert.ThrowsExactly<TestException>(_dataAccess.Reconnect);
    }

    [TestMethod]
    public void When_Reconnect_Then_PassThrough()
    {
        _dataAccess.Reconnect();
        _sharedDb.Verify(d => d.Reconnect(), Times.Once);
        _sharedDb.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Given_SharedDbWillThrow_When_CommitTransaction_Then_Throws()
    {
        _sharedDb.Setup(d => d.CommitTransaction()).Throws<TestException>();
        Assert.ThrowsExactly<TestException>(_dataAccess.CommitTransaction);
    }

    [TestMethod]
    public void When_CommitTransaction_Then_PassThrough()
    {
        _dataAccess.CommitTransaction();
        _sharedDb.Verify(d => d.CommitTransaction(), Times.Once);
        _sharedDb.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Given_SharedDbWillThrow_When_RollbackTransaction_Then_Throws()
    {
        _sharedDb.Setup(d => d.RollbackTransaction()).Throws<TestException>();
        Assert.ThrowsExactly<TestException>(_dataAccess.RollbackTransaction);
    }

    [TestMethod]
    public void When_RollbackTransaction_Then_PassThrough()
    {
        _dataAccess.RollbackTransaction();
        _sharedDb.Verify(d => d.RollbackTransaction(), Times.Once);
        _sharedDb.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Given_SharedDbWillThrow_When_RollbackToSavepoint_Then_Throws()
    {
        _sharedDb.Setup(d => d.RollbackToSavepoint(It.IsAny<string>())).Throws<TestException>();
        Assert.ThrowsExactly<TestException>(() => _dataAccess.RollbackToSavepoint("SavepointName"));
    }

    [TestMethod]
    public void When_RollbackToSavepoint_Then_PassThrough()
    {
        _dataAccess.RollbackToSavepoint("SavepointName");
        _sharedDb.Verify(d => d.RollbackToSavepoint("SavepointName"), Times.Once);
        _sharedDb.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Given_SharedDbWillThrow_When_CreateSavepoint_Then_Throws()
    {
        _sharedDb.Setup(d => d.CreateSavepoint(It.IsAny<string>())).Throws<TestException>();
        Assert.ThrowsExactly<TestException>(() => _dataAccess.CreateSavepoint("SavepointName"));
    }

    [TestMethod]
    public void When_CreateSavepoint_Then_PassThrough()
    {
        _dataAccess.CreateSavepoint("SavepointName");
        _sharedDb.Verify(d => d.CreateSavepoint("SavepointName"), Times.Once);
        _sharedDb.VerifyNoOtherCalls();
    }
}