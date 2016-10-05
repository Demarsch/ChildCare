using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using AdminModule.Services;
using Core.Extensions;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace AdminModule.ViewModels
{
    public class DatabaseValidationViewModel : BindableBase, INavigationAware
    {
        private readonly ILog log;

        private readonly IDatabaseValidtionService validationService;

        private readonly CommandWrapper startValidationCommand;

        private readonly CommandWrapper stopValidationCommand;

        public DatabaseValidationViewModel(ILog log, IDatabaseValidtionService validationService)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (validationService == null)
            {
                throw new ArgumentNullException("validationService");
            }
            this.log = log;
            this.validationService = validationService;
            startValidationCommand = new CommandWrapper { Command = new DelegateCommand(StartValidation), CommandName = "Начать" };
            stopValidationCommand = new CommandWrapper { Command = new DelegateCommand(StopValidation), CommandName = "Остановить" };
            ValidationResults = new ObservableCollectionEx<ValidationResult>();
            TotalValidationStepCount = validationService.ValidatorCount;
            currentActionCommand = startValidationCommand;
            FailureMediator = new FailureMediator();
            NotificationMediator = new NotificationMediator();
            BusyMediator = new BusyMediator();
        }

        public int TotalValidationStepCount { get; private set; }

        private int passedValidationStepCount;

        public int PassedValidationStepCount
        {
            get { return passedValidationStepCount; }
            set { SetProperty(ref passedValidationStepCount, value); }
        }

        public FailureMediator FailureMediator { get; private set; }

        public NotificationMediator NotificationMediator { get; private set; }

        public BusyMediator BusyMediator { get; private set; }

        public ObservableCollectionEx<ValidationResult> ValidationResults { get; private set; }

        private readonly object currentOperationLock = new object();

        private CancellationTokenSource currentOperationTokenSource;

        private void StopValidation()
        {
            currentOperationTokenSource.Cancel();
        }

        private async void StartValidation()
        {
            FailureMediator.Deactivate();
            lock (currentOperationLock)
            {
                CurrentActionCommand = stopValidationCommand;
                currentOperationTokenSource = new CancellationTokenSource();
                ValidationResults.Clear();
                PassedValidationStepCount = 0;
            }
            try
            {
                BusyMediator.Activate("Идет валидация...");
                await validationService.ValidateDatabaseAsync(currentOperationTokenSource.Token, new Progress<IEnumerable<ValidationResult>>(AddValidationResults));
                NotificationMediator.Activate("Валидация завершена", NotificationMediator.DefaultHideTime);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to perform database validation");
                FailureMediator.Activate("Не удалось провалидировать одну или несколько таблиц", startValidationCommand, ex, true);
            }
            finally
            {
                PassedValidationStepCount = TotalValidationStepCount;
                BusyMediator.Deactivate();
                currentOperationTokenSource.Dispose();
                CurrentActionCommand = startValidationCommand;
            }
        }

        private void AddValidationResults(IEnumerable<ValidationResult> results)
        {
            ValidationResults.AddRange(results);
            Interlocked.Increment(ref passedValidationStepCount);
            OnPropertyChanged(() => PassedValidationStepCount);
        }

        private CommandWrapper currentActionCommand;

        public CommandWrapper CurrentActionCommand
        {
            get { return currentActionCommand; }
            set { SetProperty(ref currentActionCommand, value); }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
