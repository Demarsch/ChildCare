using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using System.Linq.Expressions;
using System.Data.Entity;
using Microsoft.Practices.ServiceLocation;

namespace DataLib
{
    public class ModelAccessLayer : IDataAccessLayer
    {
        private IDataContextProvider provider;

        public ModelAccessLayer(IDataContextProvider contextProvider)
        {
            provider = contextProvider;
        }

        public virtual IList<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> predicateForEntityFields, params Expression<Func<TEntity, object>>[] navigationProperties) where TEntity : class
        {
            List<TEntity> list;
            using (IDataContext context = provider.GetNewLiteDataContext())
            {
                IQueryable<TEntity> query = context.GetData<TEntity>();
                foreach (var navigation in navigationProperties)
                    query = query.Include(navigation);
                list = query.Where(predicateForEntityFields).AsNoTracking().ToList();
            }
            return list;
        }

        public virtual IList<TEntity> Get<TEntity>(params Expression<Func<TEntity, object>>[] navigationProperties) where TEntity : class
        {
            List<TEntity> list;
            using (IDataContext context = provider.GetNewLiteDataContext())
            {
                IQueryable<TEntity> query = context.GetData<TEntity>();
                foreach (var navigation in navigationProperties)
                    query = query.Include(navigation);
                list = query.AsNoTracking().ToList();
            }
            return list;
        }

        public virtual TEntity First<TEntity>(Expression<Func<TEntity, bool>> predicateForEntityFields, params Expression<Func<TEntity, object>>[] navigationProperties) where TEntity : class
        {
            TEntity res;
            using (IDataContext context = provider.GetNewLiteDataContext())
            {
                IQueryable<TEntity> query = context.GetData<TEntity>();
                foreach (var navigation in navigationProperties)
                    query = query.Include(navigation);
                res = query.Where(predicateForEntityFields).AsNoTracking().FirstOrDefault();
            }
            return res;
        }

        public virtual void Save<TEntity>(params TEntity[] items) where TEntity : class
        {
            using (IDataContext context = provider.GetNewLiteDataContext())
            {
                foreach (TEntity item in items)
                {
                    context.SetState<TEntity>(item, (item.GetPropertyValue("Id").ToInt() > 0) ? DataContextItemState.Update : DataContextItemState.Add);
                }
                context.Save();
            }
        }

        public virtual void Delete<TEntity>(params TEntity[] items) where TEntity : class
        {
            using (IDataContext context = provider.GetNewLiteDataContext())
            {
                foreach (TEntity item in items)
                {
                    context.SetState<TEntity>(item, DataContextItemState.Delete);
                }
                context.Save();
            }
        }

        private Dictionary<Type, ObjectCache> caches = new Dictionary<Type, ObjectCache>();
        private Dictionary<Type, object[]> cachesNavigationProperties = new Dictionary<Type,object[]>();

        public void SetupCache<TEntity>(int cacheMaxSize, params Expression<Func<TEntity, object>>[] navigationPropertiesForCached) where TEntity : class
        {
            Type type = typeof(TEntity);

            caches.Remove(type);
            cachesNavigationProperties.Remove(type);

            caches.Add(type, new ObjectCache(null, cacheMaxSize));
            cachesNavigationProperties.Add(type, navigationPropertiesForCached);
        }

        public TEntity Cache<TEntity>(int entityId) where TEntity : class
        {
            Type type = typeof(TEntity);
            bool cached =  caches.ContainsKey(type);

            if (cached && caches[type].ContainsId(entityId)) return caches[type][entityId] as TEntity;

            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
            Expression expression = Expression.Property(parameter, "Id");
            expression = Expression.Equal(expression, Expression.Constant(entityId));
            Expression<Func<TEntity, bool>> resultEx = Expression.Lambda<Func<TEntity, bool>>(expression, parameter);

            TEntity item = cachesNavigationProperties.ContainsKey(type) ? First<TEntity>(resultEx, cachesNavigationProperties[type] as Expression<Func<TEntity, object>>[]) : First<TEntity>(resultEx);
            
            if (cached) caches[type][entityId] = item;

            return item;
        }

        public void ClearCache<TEntity>() where TEntity : class
        {
            Type type = typeof(TEntity);
            if (caches.ContainsKey(type)) caches[type].Clear();
        }
    }
}
