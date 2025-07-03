using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Testing;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.DatabaseSharing.Tests;

[TestClass]
public class ExternallyManagedSqlConnectionFixture
{
    private SqlConnection _nativeConnection = default!;
    private bool _nativeConncetionDisposed = false;
    private ExternallyManagedSqlConnection _connection = default!;

    [TestInitialize]
    public void Intialize()
    {
        _nativeConnection = SqlServerTesting.CreateConnection();
        _nativeConnection.Disposed += NativeConnection_Disposed;
        _connection = new ExternallyManagedSqlConnection(_nativeConnection);
    }

    [ExcludeFromCodeCoverage(Justification = "If this runs, it is a test failure.")]
    private void NativeConnection_Disposed(object? sender, System.EventArgs e)
    {
        _nativeConncetionDisposed = true;
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (_nativeConnection is not null)
            _nativeConnection.Disposed -= NativeConnection_Disposed;
    }

    [TestMethod]
    public void When_Dispose_Then_TheExternalConnectionIsNotDisposed()
    {
        _connection.Dispose();
        Assert.IsTrue(_connection.Disposed);
        Assert.IsFalse(_nativeConncetionDisposed, "The connection must not be disposed.");
    }

    [TestMethod]
    public void When_Open_Then_Throws()
    {
        Assert.ThrowsExactly<ExternallyManagedSqlConnectionException>(_connection.Open);
    }

    [TestMethod]
    public void When_Close_Then_Throws()
    {
        Assert.ThrowsExactly<ExternallyManagedSqlConnectionException>(_connection.Close);
    }

    [TestMethod]
    public void Given_ExternalTransaction_When_BeginTransction_Then_Throws()
    {
        Assert.ThrowsExactly<ExternallyManagedSqlConnectionException>(() => _ = _connection.BeginTransaction());
    }
}
