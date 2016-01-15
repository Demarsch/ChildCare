using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Core.Attributes;
using Core.Data.Misc;
using Core.Services;

namespace Core.Data.Services
{
    public class DbContextCacheService : ICacheService
    {
        private const string ExpectedIdPropertyName = "Id";

        private const string ExpectedPrimaryNamePropertyName = "Name";

        private const string ExpectedSecondaryNamePropertyName = "ShortName";

        private readonly DbContext dataContext;

        private readonly Dictionary<Type, object> itemsById;

        private readonly Dictionary<Type, object> itemsByName;

        private readonly Dictionary<Type, object> loadedTypes;

        private readonly object locker = new object();

        public DbContextCacheService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            dataContext = contextProvider.SharedContext;
            itemsById = new Dictionary<Type, object>();
            itemsByName = new Dictionary<Type, object>();
            loadedTypes = new Dictionary<Type, object>();
        }

        public TData GetItemById<TData>(int id) where TData : class
        {
            var type = typeof(TData);
            object dictionaryObj;
            lock (locker)
            {
                if (!itemsById.TryGetValue(type, out dictionaryObj))
                {
                    dictionaryObj = GetItems<TData>().ToDictionary(GetIdSelectorFunction<TData>());
                    itemsById.Add(type, dictionaryObj);
                }
            }
            var dictionary = dictionaryObj as IDictionary<int, TData>;
            TData result;
            return dictionary != null && dictionary.TryGetValue(id, out result) ? result : null;
        }

        public TData GetItemByName<TData>(string name) where TData : class
        {
            var type = typeof(TData);
            object dictionaryObj;
            lock (locker)
            {
                if (!itemsByName.TryGetValue(type, out dictionaryObj))
                {
                    dictionaryObj = GetItems<TData>().ToDictionary(GetNameSelectorFunction<TData>());
                    itemsByName.Add(type, dictionaryObj);
                }
            }
            var dictionary = dictionaryObj as IDictionary<string, TData>;
            TData result;
            return dictionary != null && dictionary.TryGetValue(name, out result) ? result : null;
        }

        public IEnumerable<TData> GetItems<TData>() where TData : class
        {
            lock (loadedTypes)
            {
                var type = typeof(TData);
                object result;
                if (!loadedTypes.TryGetValue(type, out result))
                {
                    if (type.GetCustomAttribute<NonCachableAttribute>() != null)
                    {
                        throw new InvalidOperationException(string.Format("Type {0} is marked as non-cachable", type.Name));
                    }
                    var stopWatch = Stopwatch.StartNew();
                    result = dataContext.Set<TData>().ToList();
                    stopWatch.Stop();
                    loadedTypes.Add(type, result);
                }
                return result as IEnumerable<TData>;
            }
        }

        public void AddItem<TData>(TData item) where TData : class
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            lock (loadedTypes)
            {
                dataContext.Set<TData>().Add(item);
                var changedEntities = GetChangedEntities();
                try
                {
                    dataContext.SaveChanges();
                }
                catch
                {
                    dataContext.ResetChanges<TData>();
                    throw;
                }
                ProcessChangedEntities(changedEntities);
            }
        }

        public void RemoveItem<TData>(TData item) where TData : class
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            lock (loadedTypes)
            {
                object loadedItems;
                if (!loadedTypes.TryGetValue(typeof(TData), out loadedItems))
                {
                    return;
                }
                dataContext.Set<TData>().Remove(item);
                var changedEntities = GetChangedEntities();
                try
                {
                    dataContext.SaveChanges();
                }
                catch
                {
                    dataContext.ResetChanges<TData>();
                    throw;
                }
                ProcessChangedEntities(changedEntities);
            }
        }

        public void UpdateItem<TData>(TData item) where TData : class
        {
            dataContext.ChangeTracker.DetectChanges();
            var changedEntities = GetChangedEntities();
            try
            {
                dataContext.SaveChanges();
            }
            catch
            {
                dataContext.ResetChanges<TData>();
                throw;
            }
            ProcessChangedEntities(changedEntities);
        }

        #region Internals

        private void ProcessChangedEntities(IEnumerable<ChangedEntity> changedEntities)
        {
            foreach (var changedEntity in changedEntities)
            {
                switch (changedEntity.State)
                {
                    case EntityState.Added:
                        InternalAddItem(changedEntity.Entity);
                        break;
                    case EntityState.Deleted:
                        InternalRemoveItem(changedEntity.Entity);
                        break;
                    case EntityState.Modified:
                        InternalUpdateItem(changedEntity);
                        break;
                }
            }
        }

        private void InternalUpdateItem(ChangedEntity changedEntity)
        {
            object itemByNameDictionary;
            var type = changedEntity.Entity.GetType();
            var item = changedEntity.Entity;
            var nameProperty = GetNameProperty(type);
            if (itemsByName.TryGetValue(type, out itemByNameDictionary))
            {
                var dictionary = (IDictionary)itemByNameDictionary;
                dictionary.Remove(changedEntity.OriginalValues[nameProperty.Name]);
                dictionary.Add(changedEntity.NewValues[nameProperty.Name], item);
            }
        }

        private void InternalRemoveItem(object item)
        {
            var type = item.GetType();
            object itemCollection;
            if (loadedTypes.TryGetValue(type, out itemCollection))
            {
                ((IList)itemCollection).Remove(item);
            }
            object itemByIdDictionary;
            if (itemsById.TryGetValue(type, out itemByIdDictionary))
            {
                ((IDictionary)itemByIdDictionary).Remove(GetId(item));
            }
            object itemByNameDictionary;
            if (itemsByName.TryGetValue(type, out itemByNameDictionary))
            {
                ((IDictionary)itemByNameDictionary).Remove(GetName(item));
            }
        }

        private void InternalAddItem(object item)
        {
            object itemByIdDictionary;
            var type = item.GetType();
            object itemCollection;
            if (loadedTypes.TryGetValue(type, out itemCollection))
            {
                ((IList)itemCollection).Add(item);
            }
            if (itemsById.TryGetValue(type, out itemByIdDictionary))
            {
                ((IDictionary)itemByIdDictionary).Add(GetId(item), item);
            }
            object itemByNameDictionary;
            if (itemsByName.TryGetValue(type, out itemByNameDictionary))
            {
                ((IDictionary)itemByNameDictionary).Add(GetName(item), item);
            }
        }

        private IEnumerable<ChangedEntity> GetChangedEntities()
        {
            return dataContext.ChangeTracker.Entries().Where(x => x.State == EntityState.Added || x.State == EntityState.Deleted || x.State == EntityState.Modified)
                .Select(x => new ChangedEntity(x.Entity,
                                               x.State == EntityState.Deleted || x.State == EntityState.Modified ? x.OriginalValues : null,
                                               x.State == EntityState.Added || x.State == EntityState.Modified ? x.CurrentValues : null))
                    .ToArray();
        }

        internal PropertyInfo GetIdProperty<TData>()
        {
            return GetIdProperty(typeof(TData));
        }

        internal PropertyInfo GetIdProperty(Type type)
        {
            var idPropertyAttribute = type.GetCustomAttribute<IdPropertyAttribute>();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(x => x.Name);
            PropertyInfo result;
            if (idPropertyAttribute != null && properties.TryGetValue(idPropertyAttribute.Name, out result))
            {
                return result;
            }
            if (properties.TryGetValue(ExpectedIdPropertyName, out result))
            {
                return result;
            }
            throw new ArgumentException(string.Format("Type '{0}' doesn't contain property that serves as identity property", type.Name));
        }

        internal PropertyInfo GetNameProperty<TData>()
        {
            var type = typeof(TData);
            return GetNameProperty(type);
        }

        private PropertyInfo GetNameProperty(Type type)
        {
            var namePropertyAttribute = type.GetCustomAttribute<NamePropertyAttribute>();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(x => x.Name);
            PropertyInfo result;
            if (namePropertyAttribute != null && properties.TryGetValue(namePropertyAttribute.Name, out result))
            {
                return result;
            }
            if (properties.TryGetValue(ExpectedPrimaryNamePropertyName, out result))
            {
                return result;
            }
            if (properties.TryGetValue(ExpectedSecondaryNamePropertyName, out result))
            {
                return result;
            }
            throw new ArgumentException(string.Format("Type '{0}' doesn't contain property that serves as name property", type.Name));
        }

        internal int GetId(object obj)
        {
            var type = obj.GetType();
            var idProperty = GetIdProperty(type);
            return (int)idProperty.GetValue(obj);
        }

        internal string GetName(object obj)
        {
            var type = obj.GetType();
            var nameProperty = GetNameProperty(type);
            return (string)nameProperty.GetValue(obj);
        }

        internal Func<TData, int> GetIdSelectorFunction<TData>()
        {
            var type = typeof(TData);
            var idProperty = GetIdProperty<TData>();
            var parameter = Expression.Parameter(type, "x");
            var body = Expression.Call(parameter, idProperty.GetGetMethod());
            return Expression.Lambda<Func<TData, int>>(body, parameter).Compile();
        }

        internal Func<TData, string> GetNameSelectorFunction<TData>()
        {
            var type = typeof(TData);
            var nameProperty = GetNameProperty<TData>();
            var parameter = Expression.Parameter(type, "x");
            var body = Expression.Call(parameter, nameProperty.GetGetMethod());
            return Expression.Lambda<Func<TData, string>>(body, parameter).Compile();
        }

        private class ChangedEntity
        {
            private readonly object entity;

            private readonly EntityState state;

            private readonly DbPropertyValues originalValues;

            private readonly DbPropertyValues newValues;

            public object Entity
            {
                get { return entity; }
            }

            public DbPropertyValues OriginalValues
            {
                get { return originalValues; }
            }

            public DbPropertyValues NewValues
            {
                get { return newValues; }
            }

            public EntityState State
            {
                get { return state; }
            }

            public ChangedEntity(object entity, DbPropertyValues originalValues, DbPropertyValues newValues)
            {
                this.entity = entity;
                this.state = originalValues == null ? EntityState.Added : newValues == null ? EntityState.Deleted : EntityState.Modified;
                this.originalValues = originalValues;
                this.newValues = newValues;
            }
        }

        #endregion
    }
}