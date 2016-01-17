using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Core.Data;
using Core.Expressions;
using Core.Extensions;
using Microsoft.Practices.Unity;
using Shared.Patient.Model;
using Shared.Patient.Services;

namespace Shared.Patient.Misc
{
    public static class PersonServicesInitializer
    {
        public static void Initialize(IUnityContainer container)
        {
            container.RegisterTypeIfMissing<IUserInputNormalizer, UserInputNormalizer>(new ContainerControlledLifetimeManager());
            container.RegisterTypeIfMissing<IPersonSearchService, PersonSearchService>(new ContainerControlledLifetimeManager());
            container.RegisterTypeIfMissing<IUserInputNormalizer, UserInputNormalizer>(new ContainerControlledLifetimeManager());
            container.RegisterTypeIfMissing<ISearchExpressionProvider<Person>, PersonBirthDateSearchExpressionProvider>("PersonBirthDate", new ContainerControlledLifetimeManager());
            container.RegisterTypeIfMissing<ISearchExpressionProvider<Person>, PersonIdentityDocumentNumberSearchExpressionProvider>("PersonIdentityNumber", new ContainerControlledLifetimeManager());
            container.RegisterTypeIfMissing<ISearchExpressionProvider<Person>, PersonMedNumberSearchExpressionProvider>("PersonMedNumber", new ContainerControlledLifetimeManager());
            container.RegisterTypeIfMissing<ISearchExpressionProvider<Person>, PersonNamesSearchExpressionProvider>("PersonNames", new ContainerControlledLifetimeManager());
            container.RegisterTypeIfMissing<ISearchExpressionProvider<Person>, PersonSnilsSearchExpressionProvider>("PersonSnils", new ContainerControlledLifetimeManager());
            container.RegisterTypeIfMissing<IEnumerable<ISearchExpressionProvider<Person>>, ISearchExpressionProvider<Person>[]>();
            container.RegisterTypeIfMissing<ISearchExpressionProvider<Person>, CompositeSearchExpressionProvider<Person>>();
            container.RegisterTypeIfMissing<IDocumentService, DocumentService>(new ContainerControlledLifetimeManager());
            var genericThemeSource = new Uri("pack://application:,,,/Shared.Patient;Component/Themes/Generic.xaml", UriKind.Absolute);
            if (!Application.Current.Resources.MergedDictionaries.Any(x => x.Source.Equals(genericThemeSource)))
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = genericThemeSource });
            }
        }
    }
}
