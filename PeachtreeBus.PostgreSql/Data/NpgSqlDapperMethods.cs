using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using PeachtreeBus.DatabaseSharing.PostgreSql;

namespace PeachtreeBus.Data;

public class NpgSqlDapperMethods(
    IDapperTypesHandler configureDapper,
    INpgSqlSharedDatabase database) : IDapperMethods
{
    public bool DapperConfigured { get; } = configureDapper.Configure();
    
    public Task<IEnumerable<T>> Query<T>(string statement, DynamicParameters? parameters = null)
    {
        throw new System.NotImplementedException();
    }

    public Task<T> QueryFirst<T>(string statement, DynamicParameters? parameters = null) => 
        database.Connection.QueryFirstAsync<T>(statement, parameters, database.Transaction);

    public Task<T?> QueryFirstOrDefault<T>(string statement, DynamicParameters? parameters = null)
    {
        throw new System.NotImplementedException();
    }

    public Task<T?> ExecuteScalar<T>(string statement, DynamicParameters? parameters = null)
    {
        throw new System.NotImplementedException();
    }

    public Task<int> Execute(string statement, DynamicParameters? parameters = null)
    {
        throw new System.NotImplementedException();
    }
}