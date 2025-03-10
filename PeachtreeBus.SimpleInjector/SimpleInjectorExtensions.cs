﻿using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        internal static bool IsRegistered<T>(this Container container)
        {
            return container.GetRegistration(typeof(T)) != null;
        }

        private static void RegisterConcreteTypesIfNeeded(
            this Container container,
            IEnumerable<Type> concreteMessageHandlerTypes,
            Lifestyle lifestyle)
        {
            var currentRegistrations = container.GetCurrentRegistrations().Select(ip => ip.ImplementationType);
            var needsRegistration = concreteMessageHandlerTypes.Except(currentRegistrations);
            foreach (var ct in needsRegistration)
            {
                container.Register(ct, ct, lifestyle);
            }
        }
    }
}