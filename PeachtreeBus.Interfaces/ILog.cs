namespace PeachtreeBus
{
    public interface ILog
    {
        void Debug(object message);
        void Error(object message);
        void Fatal(object message);
        void Info(object message);
        void Warn(object message);
    }

    public interface ILog<T>: ILog
    {

    }
}
