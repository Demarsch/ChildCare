using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
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
                    result = dataContext.Set<TData>().ToArray();
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
                try
                {
                    dataContext.SaveChanges();
                }
                catch
                {
                    dataContext.ResetChanges<TData>();
                    throw;
                }
                object itemByIdDictionary;
                if (itemsById.TryGetValue(typeof (TData), out itemByIdDictionary))
                {
                    ((Dictionary<int, TData>)itemByIdDictionary).Add(GetIdSelectorFunction<TData>()(item), item);
                }
                object itemByNameDictionary;
                if (itemsByName.TryGetValue(typeof (TData), out itemByNameDictionary))
                {
                    ((Dictionary<string, TData>)itemByNameDictionary).Add(GetNameSelectorFunction<TData>()(item), item);
                }
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
                if (!loadedTypes.TryGetValue(typeof (TData), out loadedItems))
                {
                    return;
                }
                dataContext.Set<TData>().Remove(item);
                try
                {
                    dataContext.SaveChanges();
                }
                catch
                {
                    dataContext.ResetChanges<TData>();
                    throw;
                }
                object itemByIdDictionary;
                if (itemsById.TryGetValue(typeof(TData), out itemByIdDictionary))
                {
                    ((Dictionary<int, TData>)itemByIdDictionary).Remove(GetIdSelectorFunction<TData>()(item));
                }
                object itemByNameDictionary;
                if (itemsByName.TryGetValue(typeof(TData), out itemByNameDictionary))
                {
                    ((Dictionary<string, TData>)itemByNameDictionary).Remove(GetNameSelectorFunction<TData>()(item));
                }
            }
        }

        public void UpdateItem<TData>(TData item) where TData : class
        {
            dataContext.ChangeTracker.DetectChanges();
            var entry = dataContext.Entry(item);
            if (entry.State != EntityState.Modified)
            {
                return;
            }
            var nameProperty = GetNameProperty<TData>();
            var originalName = nameProperty == null ? string.Empty : (string)entry.OriginalValues[nameProperty.Name];
            try
            {
                dataContext.SaveChanges();
            }
            catch
            {
                dataContext.ResetChanges<TData>();
                throw;
            }
            object itemByNameDictionary;
            if (nameProperty != null && itemsByName.TryGetValue(typeof(TData), out itemByNameDictionary))
            {
                var dictionary = (Dictionary<string, TData>)itemByNameDictionary;
                dictionary.Remove(originalName);
                dictionary.Add(GetNameSelectorFunction<TData>()(item), item);
            }
        }

        #region Internals

        internal PropertyInfo GetIdProperty<TData>()
        {
            var type = typeof(TData);
            var idPropertyAttribute = type.GetCustomAttribute<IdPropertyAttribute>();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(x => x.Name);
            PropertyInfo result;
            if (idPropertyAttribute != null && properties.TryGetValue(idPropertyAttribute.Name, out result))
            {
                return result;
            }
            return properties.TryGetValue(ExpectedIdPropertyName, out result) ? result : null;
        }

        internal PropertyInfo GetNameProperty<TData>()
        {
            var type = typeof(TData);
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
            return properties.TryGetValue(ExpectedSecondaryNamePropertyName, out result) ? result : null;
        }

        internal Func<TData, int> GetIdSelectorFunction<TData>()
        {
            var type = typeof(TData);
            var idProperty = GetIdProperty<TData>();
            if (idProperty == null)
            {
                throw new ArgumentException(string.Format("Type '{0}' doesn't contain property that serves as identity property", type.Name));
            }
            var parameter = Expression.Parameter(type, "x");
            var body = Expression.Call(parameter, idProperty.GetGetMethod());
            return Expression.Lambda<Func<TData, int>>(body, parameter).Compile();
        }

        internal Func<TData, string> GetNameSelectorFunction<TData>()
        {
            var type = typeof(TData);
            var nameProperty = GetNameProperty<TData>();
            if (nameProperty == null)
            {
                throw new ArgumentException(string.Format("Type '{0}' doesn't contain property that serves as name property", type.Name));
            }
            var parameter = Expression.Parameter(type, "x");
            var body = Expression.Call(parameter, nameProperty.GetGetMethod());
            return Expression.Lambda<Func<TData, string>>(body, parameter).Compile();
        }

        #endregion
    }
}