using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Tasks;
using PeachtreeBus.Telemetry;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class CurrentTasksFixture
{
    private CurrentTasks _tasks = default!;
    private readonly Mock<IMeters> _meters = new();

    [TestInitialize]
    public void Initialize()
    {
        _meters.Reset();
        _tasks = new(_meters.Object);
    }

    [TestMethod]
    public void Given_New_When_Count_Then_Zero()
    {
        Then_TaskCountIs(0);
    }

    [TestMethod]
    public void Given_New_When_Add_Then_CountIncreases()
    {
        var before = _tasks.Count;
        _tasks.Add(Task.Delay(1));
        Then_TaskCountIs(before + 1);
    }

    [TestMethod]
    public async Task Given_AddedTask_When_TaskCompletes_Then_CountDecreases()
    {
        var source = new TaskCompletionSource();
        _tasks.Add(source.Task);
        Then_TaskCountIs(1);
        source.SetResult();
        await _tasks.WhenAll();
        await Task.Delay(10); // give the completions time to run?
        Then_TaskCountIs(0);
    }

    [TestMethod]
    public async Task Given_MultipleAddedTask_And_WhenAll_When_AllTasksCompelte_Then_WhenAllCompletes()
    {
        var source = new TaskCompletionSource();
        _tasks.Add(source.Task);
        var source2 = new TaskCompletionSource();
        _tasks.Add(source2.Task);

        var actual = _tasks.WhenAll();
        Assert.AreNotEqual(Task.CompletedTask, actual);
        source.SetResult();
        await Task.Delay(10);
        Assert.IsFalse(actual.IsCompleted);
        source2.SetResult();
        await Task.Delay(10);
        Assert.IsTrue(actual.IsCompleted);
        await actual;
    }

    [TestMethod]
    public async Task Given_MultipleAddedTasks_And_TasksCompleted_When_WhenAll_Then_CountDecreases()
    {
        var source = new TaskCompletionSource();
        _tasks.Add(source.Task);
        var source2 = new TaskCompletionSource();
        _tasks.Add(source2.Task);

        source.SetResult();
        source2.SetResult();
        await _tasks.WhenAll();
        Then_TaskCountIs(0);
    }

    [TestMethod]
    public async Task Given_NoTasks_When_WhenAll_Then_ResultIsCompletedTask()
    {
        var actual = _tasks.WhenAll();
        Then_TaskCountIs(0);
        Assert.AreEqual(Task.CompletedTask, actual);
        await actual;
    }

    private void Then_TaskCountIs(int expectedCount)
    {
        Assert.AreEqual(expectedCount, _tasks.Count);
        Then_MetersActiveTasksIs(expectedCount);
    }

    public void Then_MetersActiveTasksIs(int expectedCount)
    {
        var actual = _meters.Invocations.Count(i => i.Method.Name == nameof(IMeters.StartTask))
            - _meters.Invocations.Count(i => i.Method.Name == nameof(IMeters.EndTask));
        Assert.AreEqual(expectedCount, actual);
    }
}
