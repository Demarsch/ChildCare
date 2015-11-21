using System.Threading.Tasks;
using Core.Wpf.Mvvm;

namespace Core.Wpf.Services
{
    public interface IDialogServiceAsync
    {
        Task<bool?> ShowDialogAsync(IDialogViewModel dialogViewModel);
    }
}