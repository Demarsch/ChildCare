using SimpleInjector;

namespace Core
{
    public class MainContainerProvider
    {
        static Container container = null;

        public static Container GetContainer()
        {
            if (container == null)
                container = new Container();
            return container;
        }
    }
}
