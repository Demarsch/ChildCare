using System;
using System.Collections.Generic;
using System.Windows;
using Core.Data;
using Core.Expressions;
using log4net;
using Microsoft.Practices.Unity;
using PatientSearchModule.Model;
using PatientSearchModule.Services;
using PatientSearchModule.Views;
using Prism.Modularity;
using Prism.Regions;
using Shell.Shared;

namespace PatientSearchModule
{
    [Module(ModuleName = WellKnownModuleNames.PatientSearchModule)]
    [ModuleDependency(WellKnownModuleNames.CoreModule)]
    public class Module : IModule
    {
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
            this.container = container.CreateChildContainer();
        }

        public void Initialize()
        {
            RegisterServices();
            RegisterViews();
        }

        private void RegisterServices()
        {
            container.RegisterInstance(LogManager.GetLogger("PATSEARCH"));

            container.RegisterType<IUserInputNormalizer, UserInputNormalizer>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISimilarityExpressionProvider<Person>, PersonBirthDateSimilarityExpressionProvider>("PersonBirthDate", new ContainerControlledLifetimeManager());
            container.RegisterType<ISimilarityExpressionProvider<Person>, PersonIdentityDocumentNumberSimilarityExpressionProvider>("PersonIdentityNumber", new ContainerControlledLifetimeManager());
            container.RegisterType<ISimilarityExpressionProvider<Person>, PersonMedNumberSimilarityExpressionProvider>("PersonMedNumber", new ContainerControlledLifetimeManager());
            container.RegisterType<ISimilarityExpressionProvider<Person>, PersonNamesSimilarityExpressionProvider>("PersonNames", new ContainerControlledLifetimeManager());
            container.RegisterType<ISimilarityExpressionProvider<Person>, PersonSnilsSimilarityExpressionProvider>("PersonSnils", new ContainerControlledLifetimeManager());
            container.RegisterType<IEnumerable<ISimilarityExpressionProvider<Person>>, ISimilarityExpressionProvider<Person>[]>();
            container.RegisterType<ISimilarityExpressionProvider<Person>, CompositeSimilarityExpressionProvider<Person>>();
            container.RegisterType<IPatientSearchService, PatientSearchService>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            regionManager.RegisterViewWithRegion(RegionNames.MainMenu, () => container.Resolve<PatientSearch>());
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"Views\FoundPatient.xaml", UriKind.Relative) });
        }
    }
}