using System;
using System.Threading.Tasks;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.DataAccessTests;

public abstract class CleanSubscribedFailedFixture : CleanFixtureBase
{
    private long _lastId = 1000;
    
    private Task Given_FailedMessage(DateTime failed) =>
        TestDataAccess.InsertSubscribedFailed(new()
        {
            Id = new(_lastId++),
            SubscriberId = SubscriberId.New(),
            ValidUntil = DateTime.UtcNow.AddDays(1),
            MessageId = UniqueIdentity.New(),
            Priority = 0,
            NotBefore = DateTime.UtcNow.AddDays(-1),
            Enqueued = DateTime.UtcNow.AddDays(-1),
            Completed = null,
            Failed = failed,
            Retries = 0,
            Headers = new(),
            Body = new("{}"),
            Topic = new("Topic"),
        });

    protected override Task Given_CountMessages(int count, DateTime finished) => 
        Repeat(() => Given_FailedMessage(finished), count);

    protected override Task<long> When_Clean(int count, DateTime before) => 
        BusDataAccess.CleanSubscribedFailed(before, count);
    
    protected override Task Then_TableHasCount(int count) => 
        TestDataAccess.Then_TableHasCount(TestConfig.SubscribedFailed,  count);
}