using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Tasks;
using PeachtreeBus.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus;

public abstract class BaseRegisterComponents
{
    public void Register(IBusConfiguration busConfiguration)
    {
        RegisterLogging();
        RegisterSpecialized();
        RegisterRequired(busConfiguration);
    }
    
    protected virtual void RegisterLogging() { }
    protected abstract void RegisterInstance<T>(T instance) where T : class;
    protected abstract void RegisterSpecialized();
    protected abstract void RegisterSingleton<TInterface, TImplementation>();
    protected abstract void RegisterScoped<TInterface, TImplementation>();

    private void RegisterRequired(IBusConfiguration busConfiguration)
    {
        RegisterInstance(busConfiguration);
        RegisterSingleton<ITaskCounter, TaskCounter>();
        RegisterScoped<ITaskManager, TaskManager>();
        RegisterSingleton<ISystemClock, SystemClock>();
        RegisterSingleton<IMeters, Meters>();
        RegisterSingleton<IAlwaysRunTracker, AlwaysRunTracker>();
        RegisterScoped<IShareObjectsBetweenScopes, ShareObjectsBetweenScopes>();
        RegisterSingleton<IDapperTypesHandler, DapperTypesHandler>();
        RegisterScoped<IBusDataAccess, DapperDataAccess>();
        RegisterScoped<ISqlConnectionFactory, SqlConnectionFactory>();
        RegisterSingleton<IProvideDbConnectionString, ProvideDbConnectionString>();
        RegisterSingleton<IRunStartupTasks, RunStarupTasks>();
        RegisterScoped<IStarters, Starters>();
    }
}
