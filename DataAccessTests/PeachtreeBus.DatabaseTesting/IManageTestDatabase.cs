namespace PeachtreeBus.DatabaseTesting;

public interface IManageTestDatabase
{
    bool Exists();
    void Create();
    void Drop();
    void Initialize();
}