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

[TestClass]
public class PostgreSqlEstimateQueuePendingFixture : EstimateQueuePendingFixture;

[TestClass]
public class PostgreSqlEstimateSubscribedPendingFixture : EstimateSubscribedPendingFixture;

[TestClass]
public class PostgreSqlExpireSubscriptionsFixture : ExpireSubscriptionsFixture;

[TestClass]
public class PostgreSqlPublishFixture : PublishFixture;

[TestClass]
public class PostgreSqlQueueMessageCompleteFixture : QueueMessageCompleteFixture;

[TestClass]
public class PostgreSqlQueueMessageFailedFixture : QueueMessageFailedFixture;

[TestClass]
public class PostgreSqlQueueMessageGetPendingFixture : QueueMessageGetPendingFixture;

[TestClass]
public class PostgreSqlQueueMessageUpdateFixture : QueueMessageUpdateFixture;

[TestClass]
public class PostgreSqlSagaInsertFixture : SagaInsertFixture;

[TestClass]
public class PostgreSqlSagaDeleteFixture : SagaDeleteFixture;

[TestClass]
public class PostgreSqlSagaUpdateFixture : SagaUpdateFixture;

[TestClass]
public class PostgreSqlSagaGetFixture : SagaGetFixture;