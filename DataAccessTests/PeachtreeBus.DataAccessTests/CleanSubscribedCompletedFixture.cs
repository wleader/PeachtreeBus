using System;
using System.Threading.Tasks;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.DataAccessTests;

public abstract class CleanSubscribedCompletedFixture : CleanFixtureBase
{
    private long _lastId = 1000;

    private Task Given_CompletedMessage(DateTime completed) =>
        TestDataAccess.InsertSubscribedCompleted(new()
        {
            Id = new(_lastId++),
            SubscriberId = SubscriberId.New(),
            ValidUntil = DateTime.UtcNow.AddDays(1),
            MessageId = UniqueIdentity.New(),
            Priority = 0,
            NotBefore = DateTime.UtcNow.AddDays(-1),
            Enqueued = DateTime.UtcNow.AddDays(-1),
            Completed = completed,
            Failed = null,
            Retries = 0,
            Headers = new(),
            Body = new("{}"),
            Topic = new("Topic"),
        });

    protected override Task Given_CountMessages(int count, DateTime finished) => 
        Repeat(() => Given_CompletedMessage(finished),  count);

    protected override Task<long> When_Clean(int count, DateTime before) => 
        BusDataAccess.CleanSubscribedCompleted(before, count);
    
    
    protected override Task Then_TableHasCount(int count) => 
        TestDataAccess.Then_TableHasCount(TestConfig.SubscribedCompleted,  count);
}