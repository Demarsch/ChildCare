namespace Core
{
    public interface IDataContextProvider
    {
        IDataContext StaticDataContext { get; }

        IDataContext GetNewDataContext();
    }
}