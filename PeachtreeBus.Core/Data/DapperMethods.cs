using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Dapper;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.Data;

public interface IDapperMethods
{
    Task<IEnumerable<T>> Query<T>(string statement, DynamicParameters? parameters = null);
    Task<T> QueryFirst<T>(string statement, DynamicParameters? parameters = null);
    Task<T?> QueryFirstOrDefault<T>(string statement, DynamicParameters? parameters = null);
    Task<T?> ExecuteScalar<T>(string statement, DynamicParameters? parameters = null);
    Task<int> Execute(string statement, DynamicParameters? parameters = null);
}

[ExcludeFromCodeCoverage(Justification = "Requires a DB Connection. Code is Exercised in DataAccess tests.")]
public class DapperMethods(
    IDapperTypesHandler configureDapper,
    ISharedDatabase database) : IDapperMethods
{
    public bool DapperConfigured { get; } = configureDapper.Configure();

    private DbConnection Conn => database.NativeConnection;
    private DbTransaction? Tran => database.NativeTransaction;
    
    public Task<IEnumerable<T>> Query<T>(string statement, DynamicParameters? parameters = null) => 
        Conn.QueryAsync<T>(statement, parameters, Tran);

    public Task<T> QueryFirst<T>(string statement, DynamicParameters? parameters = null) => 
        Conn.QueryFirstAsync<T>(statement, parameters, Tran);

    public Task<T?> QueryFirstOrDefault<T>(string statement, DynamicParameters? parameters = null) => 
        Conn.QueryFirstOrDefaultAsync<T>(statement, parameters, Tran);

    public Task<T?> ExecuteScalar<T>(string statement, DynamicParameters? parameters = null) => 
        Conn.ExecuteScalarAsync<T>(statement, parameters, Tran);

    public Task<int> Execute(string statement, DynamicParameters? parameters = null) => 
        Conn.ExecuteAsync(statement, parameters, Tran);
}