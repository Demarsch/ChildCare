using System;
using System.Collections.Generic;
using Core.Expressions;
using Core.Misc;
using Microsoft.Practices.Unity;
using PatientSearchModule.Model;
using PatientSearchModule.Views;
using Prism.Modularity;
using Prism.Regions;
using Shell.Shared;

namespace PatientSearchModule
{
    [Module(ModuleName = ModuleName)]
    public class Module : IModule
    {
        public const string ModuleName = "Patient Search Module";

        private readonly IUnityContainer container;

        private readonly IRegionManager regionManager;

        public Module(IUnityContainer container, IRegionManager regionManager)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            this.regionManager = regionManager;
            this.container = container;
        }

        public void Initialize()
        {
            RegisterServices();
            RegisterViews();
        }

        private void RegisterServices()
        {
            container.RegisterType<IUserInputNormalizer, UserInputNormalizer>(new ContainerControlledLifetimeManager());

            //container.RegisterType<ISearchExpressionProvider<>, NamesSearchExpressionProvider>(typeof(NamesSearchExpressionProvider).Name, new ContainerControlledLifetimeManager());
            //container.RegisterType<ISearchExpressionProvider<>, DatesSearchExpressionProvider>(typeof(DatesSearchExpressionProvider).Name, new ContainerControlledLifetimeManager());
            //container.RegisterType<ISearchExpressionProvider<>, DocumentNumbersSearchExpressionProvider>(typeof(DocumentNumbersSearchExpressionProvider).Name, new ContainerControlledLifetimeManager());
            //container.RegisterType<IEnumerable<ISearchExpressionProvider>, ISearchExpressionProvider[]>();
            //container.RegisterType<ISearchExpressionProvider<>, CompositeSearchExpressionProvider>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            regionManager.RegisterViewWithRegion(RegionNames.MainMenu, () => container.Resolve<PatientSearch>());
        }
    }
}