using System.Threading.Tasks;
using Microsoft.Testing.Platform.Services;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using PeachtreeBus.Management;
using PeachtreeBus.Queues;

namespace PeachtreeBus.DataAccessTests;

public abstract class ManagementDataAccessFixtureBase : DataAccessFixtureBase<IManagementDataAccess>
{
    protected IBusDataAccess BusAccess { get; private set; } = null!;
    
    public override async Task Initialize()
    {
        await base.Initialize();
        BusAccess = Scope.ServiceProvider.GetRequiredService<IBusDataAccess>();
    }

    protected async Task<QueueData> CreatePendingQueued()
    {
        var message = TestData.CreateQueueData();
        message.Id = await BusAccess.AddMessage(message, TestConfig.DefaultQueue);
        await Task.Delay(10); // make sure that messages get sequential enqueued times.
        return message;
    }

    protected async Task<QueueData> CreateCompletedQueued()
    {
        var message = await CreatePendingQueued();
        await BusAccess.CompleteMessage(message, TestConfig.DefaultQueue);
        return message;
    }
}