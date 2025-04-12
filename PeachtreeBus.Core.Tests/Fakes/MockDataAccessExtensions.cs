using Moq;
using PeachtreeBus.Data;

namespace PeachtreeBus.Core.Tests.Fakes;

public class DataAccessState
{
    public bool Connected { get; set; }
}

public static class MockDataAccessExtensions
{
    public static Mock<IBusDataAccess> DisallowTransactions(this Mock<IBusDataAccess> dataAccess)
    {
        dataAccess.Setup(d => d.BeginTransaction()).Verifiable(Times.Never);
        dataAccess.Setup(d => d.CommitTransaction()).Verifiable(Times.Never);
        dataAccess.Setup(d => d.RollbackTransaction()).Verifiable(Times.Never);
        // if the code shouldn't touch the transaction, it shouldn't touch the connection either.
        return dataAccess.DisallowReconnect();
    }

    public static Mock<IBusDataAccess> DisallowReconnect(this Mock<IBusDataAccess> dataAccess)
    {
        dataAccess.Setup(d => d.Reconnect()).Verifiable(Times.Never);
        return dataAccess;
    }
}
