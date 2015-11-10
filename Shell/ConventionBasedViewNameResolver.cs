using System;
using Core.Wpf.Services;

namespace Shell
{
    public class ConventionBasedViewNameResolver : IViewNameResolver
    {
        private const string Model = "Model";

        public string Resolve<TViewModel>()
        {
            var viewModelType = typeof(TViewModel).Name;
            if (viewModelType.EndsWith(Model))
            {
                return viewModelType.Remove(viewModelType.Length - Model.Length);
            }
            throw new Exception("View-model type name must end with 'Model' word");
        }
    }
}
