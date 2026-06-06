using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.DataAccessTests;

namespace PeachtreeBus.PostgreSql.Tests;

[TestClass]
public class PostgreSqlCleanCompletedQueueMessagesFixture : CleanCompletedQueueMessagesFixture;

[TestClass]
public class PostgreSqlQueueAddMessageFixture : QueueAddMessageFixture;

[TestClass]
public class PostgreSqlSubscriptionMessageUpdateFixture : SubscriptionMessageUpdateFixture;

[TestClass]
public class PostgreSqlCleanQueueFailedFixture : CleanQueueFailedFixture;

[TestClass]
public class PostgreSqlCleanSubscribedCompletedFixture : CleanSubscribedCompletedFixture;

[TestClass]
public class PostgreSqlCleanSubscribedFailedFixture : CleanSubscribedFailedFixture;