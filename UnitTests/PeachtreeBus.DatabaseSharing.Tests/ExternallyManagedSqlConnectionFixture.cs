using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
        _nativeConnection = GetUninitialzed<SqlConnection>();
        _nativeConnection.Disposed += _nativeConnection_Disposed;
        _connection = new ExternallyManagedSqlConnection(_nativeConnection);
    }

    [ExcludeFromCodeCoverage(Justification = "If this runs, it is a test failure.")]
    private void _nativeConnection_Disposed(object? sender, System.EventArgs e)
    {
        _nativeConncetionDisposed = true;
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (_nativeConnection is not null)
            _nativeConnection.Disposed -= _nativeConnection_Disposed;
    }


    private static T GetUninitialzed<T>()
    {
        return (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
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
