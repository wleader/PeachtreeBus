namespace PeachtreeBus.Data
{

    /// <summary>
    /// Defines the interface for IBusDataAccess Factory
    /// so that a Dependency Injection container can create an IBusDataAccess.
    /// </summary>
    public interface IBusDataAccessFactory
    {
        IBusDataAccess GetBusDataAccess();
    }
}
