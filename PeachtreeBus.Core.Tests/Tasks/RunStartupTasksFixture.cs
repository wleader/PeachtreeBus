using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Tasks;
using System;
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
    private readonly Mock<IWrappedScopeFactory> _scopeFactory = new();
    private readonly Mock<IWrappedScope> _scope1 = new();
    private readonly Mock<IWrappedScope> _scope2 = new();
    private List<Type> _implementationTypes =
    [
        typeof(FakeStartupTask1),
        typeof(FakeStartupTask2)
    ];

    private readonly FakeStartupTask1 _task1 = new();
    private readonly FakeStartupTask2 _task2 = new();

    private readonly Queue<IWrappedScope> _scopes = new();

    [TestInitialize]
    public void Initialize()
    {
        _scopeFactory.Reset();
        _scope1.Reset();
        _scope2.Reset();

        _scopeFactory.Setup(s => s.GetImplementations<IRunOnStartup>())
            .Returns(() => _implementationTypes);

        _scopes.Clear();
        _scopes.Enqueue(_scope1.Object);
        _scopes.Enqueue(_scope2.Object);

        _scopeFactory.Setup(s => s.Create())
            .Returns(_scopes.Dequeue);

        _scope1.Setup(s => s.GetInstance(typeof(FakeStartupTask1)))
            .Returns(() => _task1);
        _scope2.Setup(s => s.GetInstance(typeof(FakeStartupTask2)))
            .Returns(() => _task2);

        _task1.RunCount = 0;
        _task2.RunCount = 0;

        _runStartupTasks = new(
            _scopeFactory.Object);
    }

    [TestMethod]
    public void Given_StartupTasks_When_RunStartupTasks_Then_TasksAreRunFromScopes_And_ScopesAreDisposed()
    {
        _runStartupTasks.RunStartupTasks();

        Assert.AreEqual(1, _task1.RunCount);
        Assert.AreEqual(1, _task1.RunCount);

        _scope1.Verify(s => s.GetInstance(typeof(FakeStartupTask1)), Times.Once);
        _scope2.Verify(s => s.GetInstance(typeof(FakeStartupTask2)), Times.Once);

        _scope1.Verify(s => s.Dispose(), Times.Once);
        _scope2.Verify(s => s.Dispose(), Times.Once);
    }


    [TestMethod]
    public void Given_NoStartupTasks_When_RunStartupTasks_Then_TasksAreNotRun_And_ScopesAreNotCreated()
    {
        _implementationTypes = [];

        _runStartupTasks.RunStartupTasks();

        Assert.AreEqual(0, _task1.RunCount);
        Assert.AreEqual(0, _task1.RunCount);
        _scopeFactory.Verify(f => f.Create(), Times.Never);
    }
}
