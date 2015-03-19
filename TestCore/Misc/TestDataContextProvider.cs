using Core;

namespace TestCore
{
    public class TestDataContextProvider : IDataContextProvider
    {
        private IDataContext dataContext;

        public void SetDataContext(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        private IDataContext staticContext;

        public IDataContext StaticDataContext { get { return staticContext ?? (staticContext = GetNewDataContext()); } }

        public IDataContext GetNewDataContext()
        {
            return dataContext;
        }
    }
}
