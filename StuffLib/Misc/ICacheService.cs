using System.Collections.Generic;

namespace Core
{
    public interface ICacheService
    {
        TData GetItemById<TData>(int id) where TData : class;

        TData GetItemByName<TData>(string name) where TData : class;

        ICollection<TData> GetItems<TData>() where TData : class;
    }
}
