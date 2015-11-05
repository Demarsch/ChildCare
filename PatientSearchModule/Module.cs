using System;
using System.Collections.Generic;
using System.Windows;
using Core.Data;
using Core.Expressions;
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
            container.RegisterType<ISearchExpressionProvider<Person>, PersonBirthDateSearchExpressionProvider>("PersonBirthDate", new ContainerControlledLifetimeManager());
            container.RegisterType<ISearchExpressionProvider<Person>, PersonIdentityDocumentNumberSearchExpressionProvider>("PersonIdentityNumber", new ContainerControlledLifetimeManager());
            container.RegisterType<ISearchExpressionProvider<Person>, PersonMedNumberSearchExpressionProvider>("PersonMedNumber", new ContainerControlledLifetimeManager());
            container.RegisterType<ISearchExpressionProvider<Person>, PersonNamesSearchExpressionProvider>("PersonNames", new ContainerControlledLifetimeManager());
            container.RegisterType<ISearchExpressionProvider<Person>, PersonSnilsSearchExpressionProvider>("PersonSnils", new ContainerControlledLifetimeManager());
            container.RegisterType<IEnumerable<ISearchExpressionProvider<Person>>, ISearchExpressionProvider<Person>[]>();
            container.RegisterType<ISearchExpressionProvider<Person>, CompositeSearchExpressionProvider<Person>>();
            container.RegisterType<IPatientSearchService, PatientSearchService>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            regionManager.RegisterViewWithRegion(RegionNames.MainMenu, () => container.Resolve<PatientSearch>());
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/PatientSearchModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }
    }
}