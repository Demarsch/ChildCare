using System;
using System.Threading.Tasks;
using Core.Data.Services;
using Core.Wpf.Mvvm;
using Prism.Mvvm;

namespace Shell
{
    public class ShellWindowViewModel : BindableBase
    {
        private readonly IDbContextProvider contextProvider;

        public ShellWindowViewModel(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
        }

        public BusyMediator BusyMediator { get; private set; }

        public CriticalFailureMediator CriticalFailureMediator { get; private set; }

        public bool CanOpenMenu { get { return !CriticalFailureMediator.IsActive && !BusyMediator.IsActive; } }

        public async Task CheckDatabaseConnectionAsync()
        {
            BusyMediator.Activate("ПОДКЛЮЧЕНИЕ К БАЗЕ ДАННЫХ...");
            OnPropertyChanged(() => CanOpenMenu);
            var checkDataBaseConnectionTask = Task.Run(() => CheckDatabaseConnection());
            await Task.WhenAll(checkDataBaseConnectionTask, Task.Delay(TimeSpan.FromSeconds(1.0)));
            var success = checkDataBaseConnectionTask.Result;
            BusyMediator.Deactivate();
            if (!success)
            {
                CriticalFailureMediator.Activate("НЕ УДАЛОСЬ ПОДКЛЮЧИТЬСЯ К БАЗЕ ДАННЫХ. ОБРАТИТЕСЬ В СЛУЖБУ ПОДДЕРЖКИ");
            }
            OnPropertyChanged(() => CanOpenMenu);
        }

        private bool CheckDatabaseConnection()
        {
            using (var context = contextProvider.SharedContext)
            {
                return context.Database.Exists();
            }
        }
    }
}
