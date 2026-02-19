using Microsoft.Extensions.DependencyInjection;
using PeachtreeBus.Exceptions;
using System;

namespace PeachtreeBus.MicrosoftDependencyInjection;

public class MSDIServiceProviderAccessor : ServiceProviderAccessor<AsyncServiceScope>;

public class MSDIScopeFactoryException : PeachtreeBusException
{
    internal MSDIScopeFactoryException(string message) : base(message) { }
}

public class MSDIScopeFactory(
    IServiceProvider serviceProvider)
    : IScopeFactory
{
    public IServiceProviderAccessor Create()
    {
        var nativeScope = serviceProvider.CreateAsyncScope();
        var accessor = ServiceProviderServiceExtensions.GetService<IServiceProviderAccessor>(nativeScope.ServiceProvider);
        var msdiAccessor = (accessor as MSDIServiceProviderAccessor)
            ?? throw new MSDIScopeFactoryException(
                "IServiceProviderAccessor is not an MSDIServiceProviderAccessor. Did you replace the registration within the IServiceCollection?");
        msdiAccessor.Initialize(nativeScope, nativeScope.ServiceProvider);
        return msdiAccessor;
    }
}
