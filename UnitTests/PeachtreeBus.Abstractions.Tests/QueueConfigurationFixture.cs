using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeachtreeBus.Abstractions.Tests;

[TestClass]
public class QueueConfigurationFixture : BaseConfigurationFixture<QueueConfiguration>
{
    protected override QueueConfiguration CreateConfiguration(bool useDefaults) =>
        new()
        {
            QueueName = AbstractionsTestData.DefaultQueueName,
            UseDefaultFailedHandler = useDefaults,
            UseDefaultRetryStrategy = useDefaults,
        };

    [TestMethod]
    public void Then_QueueNameIsInit()
    {
        Assert.AreEqual(AbstractionsTestData.DefaultQueueName, _config.QueueName);
    }
}
