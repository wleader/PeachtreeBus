using System;
using System.Threading.Tasks;
using PeachtreeBus.Data;

namespace PeachtreeBus.DataAccessTests;

public abstract class CleanQueueFailedFixture : CleanFixtureBase
{
    private long _lastId = 1000;

    private Task Given_FailedMessage(DateTime failed) =>
        TestDataAccess.InsertQueueFailed(new()
        {
            Id = new(_lastId++),
            MessageId = UniqueIdentity.New(),
            Priority =0,
            NotBefore = DateTime.UtcNow.AddDays(-1),
            Enqueued = DateTime.UtcNow.AddDays(-1),
            Completed = null,
            Failed = failed,
            Retries = 0,
            Headers = new(),
            Body = new("{}"),
        });

    protected override Task Given_CountMessages(int count, DateTime finished) =>
        Repeat(() => Given_FailedMessage(finished), count);

    protected override Task<long> When_Clean(int count, DateTime before) => 
        BusDataAccess.CleanQueueFailed(TestConfig.DefaultQueue, before, count);
    
    
    protected override Task Then_TableHasCount(int count) => 
        TestDataAccess.Then_TableHasCount(TestConfig.QueueFailed,  count);
}