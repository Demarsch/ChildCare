using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface ISimpleLocator
    {
        /// <summary>
        /// Получение экземпляра класса
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService Instance<TService>() where TService : class;

        /// <summary>
        /// Добавляет экземпляр реализации интерфейса в локатор
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="service"></param>
        void Add<TService>(Type interfaceType, TService service) where TService : class;
    }
}
