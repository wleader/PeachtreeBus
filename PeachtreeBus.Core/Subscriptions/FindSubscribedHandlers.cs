﻿using PeachtreeBus.Exceptions;
using System.Collections.Generic;

namespace PeachtreeBus.Subscriptions;

/// <summary>
/// An implementation of IFindSubscribedHandlers using Simple Injector.
/// </summary>
public class FindSubscribedHandlers(
    IWrappedScope scope)
    : IFindSubscribedHandlers
{
    private readonly IWrappedScope _scope = scope;

    public IEnumerable<IHandleSubscribedMessage<T>> FindHandlers<T>() where T : ISubscribedMessage
    {
        // create the handlers in the current scope.
        // this is because when multiple threads are handling messages, each thread needs 
        // to have its own instance of the handler object, so that mulitple threads do not
        // intefere with each other.
        return _scope.GetAllInstances<IHandleSubscribedMessage<T>>()
            ?? throw new IncorrectImplementationException(
                "An IWrappedScope must not return null, when GetAllInstances is called.",
                _scope.GetType(),
                typeof(IWrappedScope));
    }
}
