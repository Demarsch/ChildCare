using Core.Wpf.Services;

namespace Shell
{
    public class ConventionBasedViewNameResolver : IViewNameResolver
    {
        public string Resolve<TViewModel>()
        {
            var viewModelType = typeof(TViewModel);
            return viewModelType.Name.Replace("ViewModel", string.Empty);
        }
    }
}
