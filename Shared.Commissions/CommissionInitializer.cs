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
using Shared.Commissions.Services;

namespace Shared.Commissions
{
    public static class CommissionInitializer
    {
        public static void Initialize(IUnityContainer container)
        {            
            container.RegisterTypeIfMissing<ICommissionService, CommissionService>(new ContainerControlledLifetimeManager());
            
            var genericThemeSource = new Uri("pack://application:,,,/Shared.Commissions;Component/Themes/Generic.xaml", UriKind.Absolute);
            if (!Application.Current.Resources.MergedDictionaries.Any(x => x.Source.Equals(genericThemeSource)))
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = genericThemeSource });
            }
        }
    }
}
