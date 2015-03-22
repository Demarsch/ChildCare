namespace Core
{
    interface IReferenceService
    {
        int GetId<TData>(string name);

        string GetName<TData>(int id);
    }
}
