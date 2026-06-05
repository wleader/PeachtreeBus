using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PeachtreeBus.DatabaseTesting;

public static class TestServices
{
    private static ServiceProvider? _serviceProvider;

    public static ServiceProvider ServiceProvider =>
        _serviceProvider ?? throw new ApplicationException("TestServices has not been initialized.");

    public static void Initialize(Action<ServiceCollection>? addServices = null)
    {
        var container = new ServiceCollection();

        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.user.json", true);
        var configurationRoot = configurationBuilder.Build();
        
        container.AddSingleton(configurationRoot);
        container.AddTransient<IBusConfiguration>(_ => new BusConfiguration
        {
            ConnectionString = configurationRoot.GetConnectionString("TestDatabase")
                ?? throw new ApplicationException("TestDatabase connection string not found."),
            Schema = new("PeachtreeBus"),
        });

        addServices?.Invoke(container);

        _serviceProvider = container.BuildServiceProvider(new ServiceProviderOptions()
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });
    }

    public static T GetService<T>() where T : class => ServiceProvider.GetRequiredService<T>();
}