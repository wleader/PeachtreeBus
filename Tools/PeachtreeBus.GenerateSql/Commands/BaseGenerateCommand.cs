using ManyConsole;
using PeachtreeBus.Data;

namespace PeachtreeBus.GenerateSql.Commands;

public abstract class BaseGenerateCommand : ConsoleCommand
{
    public SchemaName Schema { get; set; } 
    public string? Output { get; set; }

    protected void HasBaseOptions()
    {        
        HasRequiredOption("s|schema=", "The schema to use.", s => Schema = new(s));
        HasOption("o|output=", "The file name to write to. If not specified, writes to stdout.", s => Output = s);
    }

    protected void WriteOutput(string generated)
    {
        Console.Write(generated);

        if (!string.IsNullOrWhiteSpace(Output))
        {
            File.WriteAllText(Output, generated);
        }
    }
    
}