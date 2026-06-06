using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.DataAccessTests;

namespace PeachtreeBus.PostgreSql.Tests;

[TestClass]
public class PostgreSqlCleanCompletedQueueMessagesFixture : CleanCompletedQueueMessagesFixture;

[TestClass]
public class PostgreSqlQueueAddMessageFixture : QueueAddMessageFixture;

[TestClass]
public class PostgreSqlSubscriptionMessageUpdateFixture : SubscriptionMessageUpdateFixture;