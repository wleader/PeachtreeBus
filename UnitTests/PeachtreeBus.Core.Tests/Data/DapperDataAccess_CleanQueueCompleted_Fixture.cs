﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_CleanQueueCompleted_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_Parameters_When_CleanQueueCompleted_Then_TableNameIsSet()
    {
        await _dataAccess.CleanQueueCompleted(TestData.DefaultQueueName, TestData.Now, 10);
        AssertStatementContains("FROM [PBus].[DefaultQueueName_Completed]");
    }

    [TestMethod]
    public async Task Given_Parameters_When_CleanQueueCompleted_Then_ParametersAreSet()
    {
        await _dataAccess.CleanQueueCompleted(TestData.DefaultQueueName, TestData.Now, 10);
        AssertParameterSet("@MaxCount", 10);
        AssertParameterSet("@OlderThan", TestData.Now);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_CleanQueueCompleted_Then_Throws()
    {
        Given_DapperThrows<long>();
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.CleanQueueCompleted(TestData.DefaultQueueName, TestData.Now, 10));
    }
}
