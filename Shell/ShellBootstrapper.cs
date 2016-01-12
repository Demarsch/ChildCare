using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Core.Data;
using Core.Data.Services;
using Core.Misc;
using Core.Notification;
using Core.Services;
using Core.Wpf.Services;
using Fluent;
using log4net;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Shell.Shared;

namespace Shell
{
    public class ShellBootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<ShellWindow>();
        }

        protected override async void InitializeShell()
        {
            var shellWindow = (ShellWindow)Shell;
            Container.RegisterInstance(Container.Resolve<ShellWindow>().childDialogWindow);
            Application.Current.MainWindow = shellWindow;
            Application.Current.MainWindow.Show();
            var connectionEstablished = await shellWindow.ShellWindowViewModel.CheckServicesAreOnlineAsync();
            if (connectionEstablished)
            {
                await BuildCacheHierarchyAsync();
                CustomLoadModules();
            }
        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            var result = base.ConfigureRegionAdapterMappings();
            result.RegisterMapping(typeof(Ribbon), Container.Resolve<RibbonRegionAdapter>());
            return result;
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            RegisterServices();
            RegisterViews();
        }

        private async Task BuildCacheHierarchyAsync()
        {
            await Task.WhenAll(Task.Factory.StartNew(BuildRecordTypeHierarchy),
                               Task.Factory.StartNew(BuildRelativeRelationshipHierarchy),
                               Task.Factory.StartNew(BuildPermissionGroupHierarchy));
        }

        private void BuildPermissionGroupHierarchy()
        {
            var cacheService = Container.Resolve<ICacheService>();
            cacheService.GetItems<Permission>();
            cacheService.GetItems<PermissionGroup>();
            cacheService.GetItems<PermissionGroupMembership>();
            cacheService.GetItems<UserPermisionGroup>();
        }

        private void BuildRelativeRelationshipHierarchy()
        {
            var cacheService = Container.Resolve<ICacheService>();
            foreach (var relationshipConnection in cacheService.GetItems<RelativeRelationshipConnection>())
            {
                //At this point RelativeRelationshipConnection  type is loaded and cacheService will automatically attach its objects to RelativeRelationship objects
                relationshipConnection.RelativeRelationship = cacheService.GetItemById<RelativeRelationship>(relationshipConnection.ParentRelationshipId);
                relationshipConnection.RelativeRelationship1 = cacheService.GetItemById<RelativeRelationship>(relationshipConnection.ChildRelationshipId);
            }
        }

        private void BuildRecordTypeHierarchy()
        {
            var cacheService = Container.Resolve<ICacheService>();
            foreach (var recordType in cacheService.GetItems<RecordType>())
            {
                if (recordType.ParentId == null)
                {
                    continue;
                }
                var parentRecordType = cacheService.GetItemById<RecordType>(recordType.ParentId.Value);
                if (parentRecordType == null)
                {
                    continue;
                }
                if (parentRecordType.RecordTypes1 == null)
                {
                    parentRecordType.RecordTypes1 = new List<RecordType>();
                }
                recordType.RecordType1 = parentRecordType;
                parentRecordType.RecordTypes1.Add(recordType);
            }
        }

        private void RegisterViews()
        {
            Container.RegisterType<ShellWindow>(new ContainerControlledLifetimeManager());
        }

        protected override void InitializeModules()
        {
            //Do nothing as we perform custom module loading
        }

        private void CustomLoadModules()
        {
            base.InitializeModules();
            var moduleManager = Container.Resolve<IModuleManager>();
            foreach (var module in ModuleCatalog.Modules.Where(HasPermissionForModule))
            {
                moduleManager.LoadModule(module.ModuleName);
            }
        }

        private bool HasPermissionForModule(ModuleInfo moduleInfo)
        {
            var attribute = Type.GetType(moduleInfo.ModuleType).GetCustomAttribute<PermissionRequiredAttribute>();
            if (attribute == null)
            {
                return false;
            }
            var requiredPermission = attribute.PermissionName;
            if (string.IsNullOrEmpty(requiredPermission))
            {
                return true;
            }
            return Container.Resolve<ISecurityService>().HasPermission(requiredPermission);
        }

        private void RegisterServices()
        {
            Container.RegisterInstance(LogManager.GetLogger("COMMON"));
            Container.RegisterType<IDbContextProvider, DbContextProvider>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ICacheService, DbContextCacheService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IViewNameResolver, ConventionBasedViewNameResolver>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IEnvironment, DbEnvironment>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISecurityService, DbSecurityService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDialogService, WindowDialogService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDialogServiceAsync, WindowsDialogServiceAsync>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IUserService, UserService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFileService, FileService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<INotificationService, NotificationService>(new ContainerControlledLifetimeManager());
            //Resolving shared context here to avoid multithreading issue
            var context = Container.Resolve<IDbContextProvider>().SharedContext;
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog { ModulePath = @".\" };
        }
    }
}
