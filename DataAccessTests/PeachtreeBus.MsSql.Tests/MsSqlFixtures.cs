using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.DataAccessTests;

namespace PeachtreeBus.MsSql.Tests;

[TestClass]
public class MsSqlCleanCompletedQueueMessagesFixture : CleanCompletedQueueMessagesFixture;

[TestClass]
public class MsSqlQueueAddMessageFixture : QueueAddMessageFixture;

[TestClass]
public class MsSqlSubscriptionMessageUpdateFixture : SubscriptionMessageUpdateFixture;

[TestClass]
public class MsSqlCleanQueueFailedFixture : CleanQueueFailedFixture;

[TestClass]
public class MsSqlCleanSubscribedCompletedFixture : CleanSubscribedCompletedFixture;

[TestClass]
public class MsSqlCleanSubscribedFailedFixture : CleanSubscribedFailedFixture;

[TestClass]
public class MsSqlEstimateQueuePendingFixture : EstimateQueuePendingFixture;

[TestClass]
public class MsSqlEstimateSubscribedPendingFixture : EstimateSubscribedPendingFixture;
