using DotNet.Testcontainers.Containers;
using System;

namespace PeachtreeBus.DatabaseTesting;

public abstract class BaseInitializeTestDatabase(
    ITestSettings settings,
    IManageTestDatabase manageTestDatabase)
    : IInitializeTestDatabase
{
    protected IDatabaseContainer? Container;

    protected ITestSettings Settings => settings;

    protected abstract string DatabaseType { get; }

    private const string StartContainerExceptionMessage =
        "Exception initializing a containerized {0} for the tests. " +
        "Check that a Docker Engine API provider such as Docker Desktop or Podman is installed and running.";

    public void Initialize()
    {
        try
        {
            Container = BuildContainer();
            Container.StartAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            throw new ApplicationException(string.Format(StartContainerExceptionMessage, DatabaseType), ex);
        }

        settings.ServerConnectionString = Container.GetConnectionString();
        settings.ConnectionString = BuildTestConnectionString();

        if (settings.RecreateDatabase)
        {
            manageTestDatabase.Drop();
        }

        if (!manageTestDatabase.Exists())
        {
            manageTestDatabase.Create();
            manageTestDatabase.Initialize();
        }

        Assert.IsTrue(manageTestDatabase.Exists(),
            $"Test Database does not exist.");
    }

    public void Cleanup()
    {
        if (settings.ReuseContainer)
            return;

        Container?.DisposeAsync().Wait();
    }

    protected abstract IDatabaseContainer BuildContainer();

    protected abstract string BuildTestConnectionString();
}
