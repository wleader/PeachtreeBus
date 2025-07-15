using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Tasks;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class CurrentTasksFixture
{
    private CurrentTasks _tasks = default!;

    [TestInitialize]
    public void Initialize()
    {
        _tasks = new();
    }

    [TestMethod]
    public void Given_New_When_Count_Then_Zero()
    {
        Assert.AreEqual(0, _tasks.Count);
    }

    [TestMethod]
    public void Given_New_When_Add_Then_CountIncreases()
    {
        var before = _tasks.Count;
        _tasks.Add(Task.Delay(1));
        Assert.AreEqual(before + 1, _tasks.Count);
    }

    [TestMethod]
    public async Task Given_AddedTask_When_TaskCompletes_Then_CountDoesNotChange()
    {
        var source = new TaskCompletionSource();
        _tasks.Add(source.Task);
        Assert.AreEqual(1, _tasks.Count);
        source.SetResult();
        await Task.Delay(10);
        Assert.AreEqual(1, _tasks.Count);
    }


    [TestMethod]
    public async Task Given_AddedTaskCompleted_When_WhenAny_Then_CompletedTask()
    {
        var source = new TaskCompletionSource();
        _tasks.Add(source.Task);
        source.SetResult();
        await Task.Delay(10);

        var actual = _tasks.WhenAny();
        Assert.AreEqual(Task.CompletedTask, actual);
    }

    [TestMethod]
    public async Task Given_AddedTaskNotCompleted_When_WhenAny_Then_NotCompletedTask()
    {
        var source = new TaskCompletionSource();
        _tasks.Add(source.Task);

        var actual = _tasks.WhenAny();
        Assert.AreNotEqual(Task.CompletedTask, actual);
        source.SetResult();
        await actual;
    }

    [TestMethod]
    public async Task Given_MultipleAddedTask_And_WhenAny_When_TaskCompletes_Then_WhenAnyCompletes()
    {
        var source = new TaskCompletionSource();
        _tasks.Add(source.Task);
        var source2 = new TaskCompletionSource();
        _tasks.Add(source2.Task);

        var actual = _tasks.WhenAny();
        Assert.IsFalse(actual.IsCompleted);
        source.SetResult();
        await Task.Delay(10);
        Assert.IsTrue(actual.IsCompleted);
        await actual;
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
    public async Task Given_MultipleAddedTasks_And_TasksCompleted_When_WhenAny_Then_CountDecreases()
    {
        var source = new TaskCompletionSource();
        _tasks.Add(source.Task);
        var source2 = new TaskCompletionSource();
        _tasks.Add(source2.Task);

        source.SetResult();
        source2.SetResult();
        await Task.Delay(10);

        Assert.AreEqual(2, _tasks.Count);

        var whenAny = _tasks.WhenAny();

        Assert.AreEqual(0, _tasks.Count);
        await whenAny;
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
        await Task.Delay(10);

        Assert.AreEqual(2, _tasks.Count);

        var whenAll = _tasks.WhenAll();

        Assert.AreEqual(0, _tasks.Count);
        await whenAll;
    }

    [TestMethod]
    public async Task Given_NoTasks_When_WhenAll_Then_ResultIsCompletedTask()
    {
        var actual = _tasks.WhenAll();
        Assert.AreEqual(Task.CompletedTask, actual);
        await actual;
    }
}
