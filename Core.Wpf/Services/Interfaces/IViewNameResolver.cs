namespace Core.Wpf.Services
{
    public interface IViewNameResolver
    {
        string Resolve<TViewModel>();
    }
}
