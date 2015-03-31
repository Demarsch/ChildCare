namespace Core
{
    public interface ICacheService
    {
        TData GetItemById<TData>(int id) where TData : class;

        TData GetItemByName<TData>(string name) where TData : class;
    }
}
