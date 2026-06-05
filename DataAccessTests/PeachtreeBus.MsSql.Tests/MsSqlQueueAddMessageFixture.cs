using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.DataAccessTests;

namespace PeachtreeBus.MsSql.Tests;

[TestClass]
public class MsSqlQueueAddMessageFixture : QueueAddMessageFixture
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();
}