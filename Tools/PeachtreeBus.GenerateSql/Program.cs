using System;
using ManyConsole;

namespace PeachtreeBus.GenerateSql;

internal class Program
{
    public static int Main(string[] args)
    {
        var commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
        return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
    } 
}