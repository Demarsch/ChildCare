using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Core
{
    public class DataContextCacheService : ICacheService
    {
        public static readonly string ExpectedIdPropertyName = "Id";

        public static readonly string ExpectedPrimaryNamePropertyName = "Name";

        public static readonly string ExpectedSecondaryNamePropertyName = "ShortName";

        private readonly IDataContext dataContext;

        private readonly Dictionary<Type, object> itemsById;

        private readonly Dictionary<Type, object> itemsByName;


        public DataContextCacheService(IDataContextProvider dataContextProvider)
        {
            if (dataContextProvider == null)
                throw new ArgumentNullException("dataContextProvider");
            dataContext = dataContextProvider.StaticDataContext;
            itemsById = new Dictionary<Type, object>();
            itemsByName = new Dictionary<Type, object>();
        }

        public TData GetItemById<TData>(int id) where TData : class
        {
            var type = typeof(TData);
            object dictionaryObj;
            if (!itemsById.TryGetValue(type, out dictionaryObj))
            {
                dictionaryObj = dataContext.GetData<TData>().ToDictionary(GetIdSelectorFunction<TData>());
                itemsById.Add(type, dictionaryObj);
            }
            var dictionary = dictionaryObj as IDictionary<int, TData>;
            TData result;
            return dictionary.TryGetValue(id, out result) ? result : null;
        }

        public TData GetItemByName<TData>(string name) where TData : class
        {
            var type = typeof(TData);
            object dictionaryObj;
            if (!itemsByName.TryGetValue(type, out dictionaryObj))
            {
                dictionaryObj = dataContext.GetData<TData>().ToDictionary(GetNameSelectorFunction<TData>());
                itemsByName.Add(type, dictionaryObj);
            }
            var dictionary = dictionaryObj as IDictionary<string, TData>;
            TData result;
            return dictionary.TryGetValue(name, out result) ? result : null;
        }

        public ICollection<TData> GetItems<TData>() where TData : class
        {
            return dataContext.GetData<TData>().ToArray();
        }

        #region Internals

        internal PropertyInfo GetIdProperty<TData>()
        {
            var type = typeof(TData);
            var idPropertyAttribute = type.GetCustomAttribute<IdPropertyAttribute>();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(x => x.Name);
            PropertyInfo result;
            if (idPropertyAttribute != null && properties.TryGetValue(idPropertyAttribute.Name, out result))
                return result;
            return properties.TryGetValue(ExpectedIdPropertyName, out result) ? result : null;
        }

        internal PropertyInfo GetNameProperty<TData>()
        {
            var type = typeof(TData);
            var namePropertyAttribute = type.GetCustomAttribute<NamePropertyAttribute>();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(x => x.Name);
            PropertyInfo result;
            if (namePropertyAttribute != null && properties.TryGetValue(namePropertyAttribute.Name, out result))
                return result;
            if (properties.TryGetValue(ExpectedPrimaryNamePropertyName, out result))
                return result;
            return properties.TryGetValue(ExpectedSecondaryNamePropertyName, out result) ? result : null;
        }

        internal Func<TData, int> GetIdSelectorFunction<TData>()
        {
            var type = typeof(TData);
            var idProperty = GetIdProperty<TData>();
            if (idProperty == null)
                throw new ArgumentException(string.Format("Type '{0}' doesn't contain property that serves as identity property", type.Name));
            var parameter = Expression.Parameter(type, "x");
            var body = Expression.Call(parameter, idProperty.GetGetMethod());
            return Expression.Lambda<Func<TData, int>>(body, parameter).Compile();
        }

        internal Func<TData, string> GetNameSelectorFunction<TData>()
        {
            var type = typeof(TData);
            var nameProperty = GetNameProperty<TData>();
            if (nameProperty == null)
                throw new ArgumentException(string.Format("Type '{0}' doesn't contain property that serves as name property", type.Name));
            var parameter = Expression.Parameter(type, "x");
            var body = Expression.Call(parameter, nameProperty.GetGetMethod());
            return Expression.Lambda<Func<TData, string>>(body, parameter).Compile();
        }

        #endregion
    }
}