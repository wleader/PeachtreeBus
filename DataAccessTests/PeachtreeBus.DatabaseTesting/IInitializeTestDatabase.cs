namespace PeachtreeBus.DatabaseTesting;

public interface IInitializeTestDatabase
{
    void Initialize();
    void Cleanup();
}