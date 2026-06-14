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

[TestClass]
public class PostgreSqlSubscriptionExpireMessagesFixture : SubscriptionExpireMessagesFixture;

[TestClass]
public class PostgreSqlSubscribeFixture : SubscribeFixture;

[TestClass]
public class PostgreSqlSubscriptionMessageCompleteFixture : SubscriptionMessageCompleteFixture;

[TestClass]
public class PostgreSqlSubscriptionMessageGetPendingFixture : SubscriptionMessageGetPendingFixture;

[TestClass]
public class PostgreSqlSubscriptionMessageFailedFixture : SubscriptionMessageFailedFixture;

[TestClass]
public class PostgreSqlManagementGetPendingQueueMessagesFixture : ManagementGetPendingQueueMessagesFixture;

[TestClass]
public class PostgreSqlManagementCancelQueueMessageFixture : ManagementCancelQueueMessageFixture;

[TestClass]
public class PostgreSqlManagementGetCompletedQueueMessagesFixture : ManagementGetCompletedQueueMessagesFixture;

[TestClass]
public class PostgreSqlManagementCancelSubscribedMessageFixture : ManagementCancelSubscribedMessageFixture;

[TestClass]
public class PostgreSqlManagementGetCompletedSubscribedMessagesFixture : ManagementGetCompletedSubscribedMessagesFixture;

[TestClass]
public class PostgreSqlManagementGetFailedQueueMessagesFixture : ManagementGetFailedQueueMessagesFixture;

[TestClass]
public class PostgreSqlManagementGetFailedSubscribedMessagesFixture : ManagementGetFailedSubscribedMessagesFixture;