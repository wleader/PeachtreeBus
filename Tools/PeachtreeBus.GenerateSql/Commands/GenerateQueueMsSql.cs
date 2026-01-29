using System.Text;

namespace PeachtreeBus.GenerateSql.Commands;

public class GenerateQueueMsSql : BaseGenerateCommand
{
    public Queues.QueueName Queue { get; set; }
    
    public GenerateQueueMsSql()
    {
        IsCommand("GenerateQueueMsSql", "Generates script to create queue tables for Microsoft Sql Server");
        HasBaseOptions();
        HasRequiredOption("n|name=", "The name of the queue.", s => Queue = new(s));
    }
    
    public override int Run(string[] remainingArguments)
    {
        var builder = new StringBuilder();
        
        AppendTable(builder, "Completed", false);
        AppendTable(builder, "Failed",false);
        AppendTable(builder, "Pending", true);
        
        // Pending table has an extra index that the others don't
        builder.AppendLine(
            $"CREATE INDEX IX_{Schema}_{Queue.ToMsSqlConstraint()}_Pending_GetNext ON [{Schema}].[{Queue}_Pending] ([Priority]) INCLUDE ([NotBefore])");
        builder.AppendLine("GO");
        builder.AppendLine();

        WriteOutput(builder.ToString());

        return 0;
    }

    private void AppendTable(StringBuilder builder, string table, bool identity)
    {
        var identityStr = identity ? " IDENTITY" : "";
        builder
            .AppendLine($"CREATE TABLE [{Schema}].[{Queue}_{table}]")
            .AppendLine("(")
            .Indent(1).Append("[Id] BIGINT NOT NULL").Append(identity).AppendLine(",")
            .IndentLine("[MessageId] UNIQUEIDENTIFIER NOT NULL,", 1)
            .IndentLine("[Priority] INT NOT NULL,", 1)
            .IndentLine("[NotBefore] DATETIME2 NOT NULL,", 1)
            .IndentLine("[Enqueued] DATETIME2 NOT NULL,", 1)
            .IndentLine("[Completed] DATETIME2 NULL,", 1)
            .IndentLine("[Failed] DATETIME2 NULL,", 1)
            .IndentLine("[Retries] TINYINT NOT NULL,", 1)
            .IndentLine("[Headers] NVARCHAR(MAX) NOT NULL,", 1)
            .IndentLine("[Body] NVARCHAR(MAX) NOT NULL,", 1)
            .IndentLine($"CONSTRAINT PK_{Schema}_{Queue.ToMsSqlConstraint()}_{table}_Id PRIMARY KEY ([Id])", 1)
            .AppendLine(")")
            .AppendLine("GO")
            .AppendLine()
            .AppendLine(
                $"ALTER TABLE [{Schema}].[{Queue}_{table}] ADD CONSTRAINT DF_{Schema}_{Queue.ToMsSqlConstraint()}_{table}_Retries DEFAULT ((0)) FOR [Retries]")
            .AppendLine("GO")
            .AppendLine()
            .AppendLine(
                $"CREATE INDEX IX_{Schema}_{Queue.ToMsSqlConstraint()}_{table}_Enqueued ON [{Schema}].[{Queue}_{table}] ([Enqueued] DESC)")
            .AppendLine("GO")
            .AppendLine();
    }
}

