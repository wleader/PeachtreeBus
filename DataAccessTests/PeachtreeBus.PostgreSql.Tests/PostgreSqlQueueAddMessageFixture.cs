using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.DataAccessTests;
namespace PeachtreeBus.PostgreSql.Tests;

[TestClass]
public class PostgreSqlQueueAddMessageFixture : QueueMessageAddFixtureBase
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();
}