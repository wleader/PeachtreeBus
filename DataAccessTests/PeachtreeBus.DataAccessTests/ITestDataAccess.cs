using System.Collections.Generic;
using System.Data;
using PeachtreeBus.Data;

namespace PeachtreeBus.DataAccessTests;

public interface ITestDataAccess
{
    void Initialize();
    void CleanEverything();
    void CloseConnections();
    long CountRowsInTable(TableName tableName);
    DataSet GetTableContent(TableName tableName);
    List<T> GetTableContent<T>(TableName tableName) where T : class;
}