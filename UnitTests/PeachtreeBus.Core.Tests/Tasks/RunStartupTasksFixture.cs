using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class RunStartupTasksFixture
{
    public abstract class FakeStartupTask : IRunOnStartup
    {
        public int RunCount { get; set; }
        public Task Run()
        {
            RunCount++;
            return Task.CompletedTask;
        }
    }

    public class FakeStartupTask1 : FakeStartupTask;
    public class FakeStartupTask2 : FakeStartupTask;

    private RunStarupTasks _runStartupTasks = default!;
    private readonly Mock<IScopeFactory> _scopeFactory = new();
    private readonly Mock<IBusConfiguration> _busConfiguration = new();
    private readonly FakeServiceProviderAccessor _accessor = new();

    private readonly FakeStartupTask1 _task1 = new();
    private readonly FakeStartupTask2 _task2 = new();

    [TestInitialize]
    public void Initialize()
    {
        _busConfiguration.Reset();
        _scopeFactory.Reset();
        _accessor.Reset();

        _busConfiguration.SetupGet(c => c.UseStartupTasks).Returns(true);

        _scopeFactory.Setup(s => s.Create())
            .Returns(_accessor);

        _accessor.SetupService<IEnumerable<IRunOnStartup>>(() => [_task1, _task2]);

        _task1.RunCount = 0;
        _task2.RunCount = 0;

        _runStartupTasks = new(
            _busConfiguration.Object,
            _scopeFactory.Object);
    }

    [TestMethod]
    public void Given_StartupTasks_When_RunStartupTasks_Then_TasksAreRunFromScopes_And_ScopesAreDisposed()
    {
        _runStartupTasks.RunStartupTasks();

        Assert.AreEqual(1, _task1.RunCount);
        Assert.AreEqual(1, _task1.RunCount);
        _accessor.ServiceProviderMock.Verify(s => s.GetService(typeof(IEnumerable<IRunOnStartup>)), Times.Once);
        _accessor.Mock.Verify(s => s.Dispose(), Times.Once);
    }

    [TestMethod]
    public void Given_StartupTasks_And_UseStartupTasksFalse_When_RunStartupTasks_Then_TasksAreNotRun_And_ScopesAreNotCreated()
    {
        _busConfiguration.SetupGet(c => c.UseStartupTasks).Returns(false);
        _runStartupTasks.RunStartupTasks();

        Assert.AreEqual(0, _task1.RunCount);
        Assert.AreEqual(0, _task1.RunCount);
        _scopeFactory.Verify(f => f.Create(), Times.Never);
    }


    [TestMethod]
    public void Given_NoStartupTasks_When_RunStartupTasks_Then_TasksAreNotRun()
    {
        _accessor.SetupService<IEnumerable<IRunOnStartup>>(() => []);

        _runStartupTasks.RunStartupTasks();

        Assert.AreEqual(0, _task1.RunCount);
        Assert.AreEqual(0, _task1.RunCount);
        _scopeFactory.Verify(f => f.Create(), Times.Once);
    }
}
