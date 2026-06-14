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

[TestClass]
public class MsSqlExpireSubscriptionsFixture : ExpireSubscriptionsFixture;

[TestClass]
public class MsSqlPublishFixture : PublishFixture;

[TestClass]
public class MsSqlQueueMessageCompleteFixture : QueueMessageCompleteFixture;

[TestClass]
public class MsSqlQueueMessageFailedFixture : QueueMessageFailedFixture;

[TestClass]
public class MsSqlQueueMessageGetPendingFixture : QueueMessageGetPendingFixture;

[TestClass]
public class MsSqlQueueMessageUpdateFixture : QueueMessageUpdateFixture;

[TestClass]
public class MsSqlSagaInsertFixture : SagaInsertFixture;

[TestClass]
public class MsSqlSagaDeleteFixture : SagaDeleteFixture;

[TestClass]
public class MsSqlSagaUpdateFixture : SagaUpdateFixture;

[TestClass]
public class MsSqlSagaGetFixture : SagaGetFixture;

[TestClass]
public class MsSqlSubscriptionExpireMessagesFixture : SubscriptionExpireMessagesFixture;

[TestClass]
public class MsSqlSubscribeFixture : SubscribeFixture;

[TestClass]
public class MsSqlSubscriptionMessageCompleteFixture : SubscriptionMessageCompleteFixture;

[TestClass]
public class MsSqlSubscriptionMessageGetPendingFixture : SubscriptionMessageGetPendingFixture;

[TestClass]
public class MsSqlSubscriptionMessageFailedFixture : SubscriptionMessageFailedFixture;

[TestClass]
public class MsSqlManagementGetPendingQueueMessagesFixture : ManagementGetPendingQueueMessagesFixture;

[TestClass]
public class MsSqlManagementCancelQueueMessageFixture : ManagementCancelQueueMessageFixture;

[TestClass]
public class MsSqlManagementGetCompletedQueueMessagesFixture : ManagementGetCompletedQueueMessagesFixture;

[TestClass]
public class MsSqlManagementCancelSubscribedMessageFixture : ManagementCancelSubscribedMessageFixture;

[TestClass]
public class MsSqlManagementGetCompletedSubscribedMessagesFixture : ManagementGetCompletedSubscribedMessagesFixture;

[TestClass]
public class MsSqlManagementGetFailedQueueMessagesFixture : ManagementGetFailedQueueMessagesFixture;

[TestClass]
public class MsSqlManagementGetFailedSubscribedMessagesFixture : ManagementGetFailedSubscribedMessagesFixture;

[TestClass]
public class MsSqlManagementGetPendingSubscribedMessagesFixture : ManagementGetPendingSubscribedMessagesFixture;