using System;
using Core.Data.Services;
using Core.Services;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Shell.Shared;

namespace CoreModule
{
    [Module(ModuleName = WellKnownModuleNames.CoreModule)]
    public class Module : IModule
    {
        private readonly IUnityContainer container;

        public Module(IUnityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            this.container = container;
        }

        public void Initialize()
        {
            RegisterServices();
        }

        private void RegisterServices()
        {
            container.RegisterType<IDbContextProvider, DbContextProvider>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICacheService, DbContextCacheService>(new ContainerControlledLifetimeManager());
        }
    }
}
