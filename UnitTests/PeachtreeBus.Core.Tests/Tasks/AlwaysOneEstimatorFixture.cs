using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Tasks;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class AlwaysOneEstimatorFixture
{
    [TestMethod]
    public async Task When_EstimateDemand_Then_ResultIsOne()
    {
        var estimator = new AlwaysOneEstimator();
        var actual = await estimator.EstimateDemand();
        Assert.AreEqual(1, actual);
    }
}
