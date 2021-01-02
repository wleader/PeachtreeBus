using System;

namespace PeachtreeBus.Services
{
    public class ConsoleLog : ConsoleLog<object>, ILog { }
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
