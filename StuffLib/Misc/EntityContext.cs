using System;

namespace Core
{
    public class EntityContext<TData> : IDisposable where TData : class
    {
        public TData Entity { get; private set; }

        public IDataContext Context { get; private set; }

        public EntityContext(TData entity, IDataContext context)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            if (context == null)
                throw new ArgumentNullException("context");
            Context = context;
            Entity = entity;
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}
