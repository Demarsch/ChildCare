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
    }
}
