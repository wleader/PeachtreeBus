using Dapper;
using PeachtreeBus.DatabaseSharing;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.Data;

public interface IDapperMethods
{
    Task<IEnumerable<T>> Query<T>(string statement, DynamicParameters? parameters = null);
    Task<T> QueryFirst<T>(string statement, DynamicParameters parameters);
    Task<T?> QueryFirstOrDefault<T>(string statement, DynamicParameters? parameters = null);
    Task<T?> ExecuteScalar<T>(string statement, DynamicParameters? parameters = null);
    Task<int> Execute(string statement, DynamicParameters? parameters = null);
}

[ExcludeFromCodeCoverage(Justification = "Requires a live database connection.")]
public class DapperMethods(
     IDapperTypesHandler configureDapper,
     ISharedDatabase database)
    : IDapperMethods
{
    public bool DapperConfigured { get; } = configureDapper.Configure();


    public Task<IEnumerable<T>> Query<T>(string statement, DynamicParameters? parameters = null)
    {
        return database.Connection.QueryAsync<T>(statement, parameters, database.Transaction);
    }

    public Task<T> QueryFirst<T>(string statement, DynamicParameters parameters)
    {
        return database.Connection.QueryFirstAsync<T>(statement, parameters, database.Transaction);
    }

    public Task<T?> QueryFirstOrDefault<T>(string statement, DynamicParameters? parameters = null)
    {
        return database.Connection.QueryFirstOrDefaultAsync<T>(statement, parameters, database.Transaction);
    }

    public Task<T?> ExecuteScalar<T>(string statement, DynamicParameters? parameters = null)
    {
        return database.Connection.ExecuteScalarAsync<T>(statement, parameters, database.Transaction);
    }

    public Task<int> Execute(string statement, DynamicParameters? parameters = null)
    {
        return database.Connection.ExecuteAsync(statement, parameters, database.Transaction);
    }
}
