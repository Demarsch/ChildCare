using System.Collections.Generic;
using DataLib;

namespace Core
{
    public interface IUserSystemInfoService
    {
        /// <summary>
        /// поиск пользователей
        /// </summary>
        /// <param name="someUserIdentity">имя или логин для поиска</param>
        /// <returns></returns>
        IList<UserSystemInfo> Find(string someUserName);

        /// <summary>
        /// возвращает SID текущего пользователя
        /// </summary>
        /// <returns></returns>
        string GetCurrentSID();

        /// <summary>
        /// поиск пользователя по SID
        /// </summary>
        /// <param name="SID">SID</param>
        /// <returns></returns>
        UserSystemInfo GetBySID(string SID);
    }
}
