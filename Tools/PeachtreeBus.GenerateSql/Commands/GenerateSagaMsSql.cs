using System.Text;
using ManyConsole;
using PeachtreeBus.Sagas;

namespace PeachtreeBus.GenerateSql.Commands;

public class GenerateSagaMsSql : BaseGenerateCommand
{
    public SagaName Saga { get; set; }
    
    public GenerateSagaMsSql()
    {
        IsCommand("GenerateSagaMsSql", "Generates script to create a saga data table for Microsoft Sql Server");
        HasBaseOptions();
        HasRequiredOption("n|name=", "The name of the saga.", s => Saga = new(s));
    }

    public override int Run(string[] remainingArguments)
    {
        var builder = new StringBuilder();
        builder
            .AppendLine($"CREATE TABLE [{Schema}].[{Saga}_SagaData]")
            .AppendLine("(")
            .IndentLine("[Id] BIGINT NOT NULL IDENTITY,", 1)
            .IndentLine("[SagaId] UNIQUEIDENTIFIER NOT NULL,", 1)
            .IndentLine("[Key] NVARCHAR(128) NOT NULL,", 1)
            .IndentLine("[Data] NVARCHAR(MAX) NOT NULL,", 1)
            .IndentLine("[MetaData] NVARCHAR(MAX) NOT NULL,", 1)
            .IndentLine($"CONSTRAINT PK_{Schema}_{Saga}_SagaData_Id PRIMARY KEY ([Id])", 1)
            .AppendLine(")")
            .AppendLine("GO")
            .AppendLine()
            .AppendLine($"CREATE UNIQUE INDEX IX_{Schema}_{Saga}_SagaData_Key ON [{Schema}].[{Saga}_SagaData] ([Key])")
            .AppendLine("GO");

        WriteOutput(builder.ToString());

        return 0;
    }
}
