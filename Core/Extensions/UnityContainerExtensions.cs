using System;
using Microsoft.Practices.Unity;

namespace Core.Extensions
{
    public static class UnityContainerExtensions
    {
        public static IUnityContainer RegisterTypeIfMissing<TInterface, TImplementation>(this IUnityContainer container) where TImplementation : TInterface
        {
            return container.RegisterTypeIfMissing<TInterface, TImplementation>(new TransientLifetimeManager());
        }

        public static IUnityContainer RegisterTypeIfMissing<TInterface, TImplementation>(this IUnityContainer container, LifetimeManager lifetimeManager) where TImplementation : TInterface
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (!container.IsRegistered<TInterface>())
            {
                container.RegisterType<TInterface, TImplementation>(lifetimeManager);
            }
            return container;
        }

        public static IUnityContainer RegisterTypeIfMissing<TInterface, TImplementation>(this IUnityContainer container, string name, LifetimeManager lifetimeManager) where TImplementation : TInterface
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (!container.IsRegistered<TInterface>())
            {
                container.RegisterType<TInterface, TImplementation>(name, lifetimeManager);
            }
            return container;
        }
    }
}
