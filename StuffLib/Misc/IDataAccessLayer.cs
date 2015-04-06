using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace DataLib
{
    public interface IDataAccessLayer
    {
        /// <summary>
        /// Получает объекты по условию, подгружает указанные зависимости
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="predicateForEntityFields">условие по свойствам объекта или указанных зависимостей</param>
        /// <param name="navigationProperties">подгружаемые зависимости</param>
        /// <returns></returns>
        IList<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> predicateForEntityFields, params Expression<Func<TEntity, object>>[] navigationProperties) where TEntity : class;

        /// <summary>
        /// Получает первый объект по условию, подгружает указанные зависимости
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="predicateForEntityFields">условие по свойствам объекта или указанных зависимостей</param>
        /// <param name="navigationProperties">подгружаемые зависимости</param>
        /// <returns></returns>
        TEntity First<TEntity>(Expression<Func<TEntity, bool>> predicateForEntityFields, params Expression<Func<TEntity, object>>[] navigationProperties) where TEntity : class;

        /// <summary>
        /// Добавляет или обновляет объекты с вызовом SaveChanges
        /// </summary>
        /// <param name="items"></param>
        void Save<TEntity>(params TEntity[] items) where TEntity : class;

        /// <summary>
        /// Удаляет объекты с вызовом SaveChanges
        /// </summary>
        /// <param name="items"></param>
        void Delete<TEntity>(params TEntity[] items) where TEntity : class;

        /// <summary>
        /// Установка параметров кеширования
        /// </summary>
        /// <param name="cacheMaxSize">размер кеша</param>
        /// <param name="navigationPropertiesForCached">подружаемые зависимости для метода Cache</param>
        void SetupCache<TEntity>(int cacheMaxSize, params Expression<Func<TEntity, object>>[] navigationPropertiesForCached) where TEntity : class;

        /// <summary>
        /// Кешированное получение объекта по свойству Id
        /// </summary>
        /// <param name="entityId">значение для Id</param>
        /// <returns></returns>
        TEntity Cache<TEntity>(int entityId) where TEntity : class;

        /// <summary>
        /// Очищает кеш
        /// </summary>
        void ClearCache<TEntity>() where TEntity : class;
        
    }
}
