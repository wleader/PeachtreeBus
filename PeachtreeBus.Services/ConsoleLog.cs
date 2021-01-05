using System;

namespace PeachtreeBus.Services
{
    /// <summary>
    /// An implementation of ILog that writes to the console.
    /// </summary>
    public class ConsoleLog : ConsoleLog<object>, ILog { }

    /// <summary>
    /// An implementation of ILog&lt;T&gt; that writes to the console.
    /// </summary>
    public class ConsoleLog<T> : ILog<T>
    {
        public void Debug(object message)
        {
            Console.WriteLine(message);
        }

        public void Error(object message)
        {
            Console.WriteLine(message);
        }

        public void Fatal(object message)
        {
            Console.WriteLine(message);
        }

        public void Info(object message)
        {
            Console.WriteLine(message);
        }

        public void Warn(object message)
        {
            Console.WriteLine(message);
        }
    }
}
