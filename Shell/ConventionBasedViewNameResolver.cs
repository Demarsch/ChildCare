using System;
using Core.Wpf.Services;

namespace Shell
{
    public class ConventionBasedViewNameResolver : IViewNameResolver
    {
        private const string Model = "Model";

        public string Resolve<TViewModel>()
        {
            var viewModelType = typeof(TViewModel).FullName;
            if (viewModelType.EndsWith(Model))
            {
                var result = viewModelType.Remove(viewModelType.Length - Model.Length)
                                           .Replace("ViewModels", "Views");
                return result;
            }
            throw new Exception("View-model type name must end with 'Model' word");
        }
    }
}
