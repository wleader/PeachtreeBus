using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.DataAccessTests;

namespace PeachtreeBus.MsSql.Tests;

[TestClass]
public class MsSqlCleanCompletedQueueMessagesFixture : CleanCompletedQueueMessagesFixture;

[TestClass]
public class MsSqlQueueAddMessageFixture : QueueAddMessageFixture;

