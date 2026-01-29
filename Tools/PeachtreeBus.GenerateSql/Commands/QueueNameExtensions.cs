using PeachtreeBus.Queues;

namespace PeachtreeBus.GenerateSql.Commands;

public static class QueueNameExtensions
{
    public static string ToMsSqlConstraint(this QueueName queueName) => 
        queueName.Value.Replace('.', '_');
}