using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Testing;

namespace PeachtreeBus.DatabaseSharing.MsSql.Tests;

[TestClass]
public class ExternallyManagedSqlConnectionFixture
{
    private SqlConnection _nativeConnection = null!;
    private bool _nativeConnectionDisposed;
    private ExternallyManagedSqlConnection _connection = null!;

    [TestInitialize]
    public void Initialize()
    {
        _nativeConnection = UninitializedObjects.Create<SqlConnection>();
        _nativeConnection.Disposed += NativeConnection_Disposed;
        _connection = new ExternallyManagedSqlConnection(_nativeConnection);
    }

    [ExcludeFromCodeCoverage(Justification = "If this runs, it is a test failure.")]
    private void NativeConnection_Disposed(object? sender, System.EventArgs e)
    {
        _nativeConnectionDisposed = true;
    }

    [TestCleanup]
    public void Cleanup()
    {
        _nativeConnection.Disposed -= NativeConnection_Disposed;
    }

    [TestMethod]
    public void When_Dispose_Then_TheExternalConnectionIsNotDisposed()
    {
        _connection.Dispose();
        Assert.IsTrue(_connection.Disposed);
        Assert.IsFalse(_nativeConnectionDisposed, "The connection must not be disposed.");
    }

    [TestMethod]
    public void When_Open_Then_Throws()
    {
        Assert.ThrowsExactly<ExternallyManagedConnectionException>(_connection.Open);
    }

    [TestMethod]
    public void When_Close_Then_Throws()
    {
        Assert.ThrowsExactly<ExternallyManagedConnectionException>(_connection.Close);
    }

    [TestMethod]
    public void Given_ExternalTransaction_When_BeginTransaction_Then_Throws()
    {
        Assert.ThrowsExactly<ExternallyManagedConnectionException>(() => _ = _connection.BeginTransaction());
    }
}