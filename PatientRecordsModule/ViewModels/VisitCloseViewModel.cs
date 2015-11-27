using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.PopupWindowActionAware;
using Core.Wpf.Services;
using log4net;
using PatientRecordsModule.DTO;
using PatientRecordsModule.Services;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Data.Entity;
using Core.Extensions;
using Core.Wpf.Misc;
using Core.Misc;
using WpfControls.Editors;
using Microsoft.Practices.Unity;
using PatientRecordsModule.Misc;

namespace PatientRecordsModule.ViewModels
{
    public class VisitCloseViewModel : TrackableBindableBase, INotification, IPopupWindowActionAware, IDataErrorInfo, IChangeTrackerMediator, IDisposable
    {
        #region Fields
        private readonly IPatientRecordsService patientRecordsService;
        private readonly ILog logService;

        private CancellationTokenSource currentOperationToken;
        private readonly CommandWrapper reloadVisitDataCommandWrapper;
        private readonly CommandWrapper saveChangesCommandWrapper;

        private int visitId = 0;
        #endregion

        #region Constructors
        public VisitCloseViewModel(IPatientRecordsService patientRecordsService, ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            this.patientRecordsService = patientRecordsService;
            this.logService = logService;
            ChangeTracker = new ChangeTrackerEx<VisitCloseViewModel>(this);
            ChangeTracker.PropertyChanged += OnChangesTracked;
            VisitResults = new ObservableCollectionEx<CommonIdName>();
            VisitOutcomes = new ObservableCollectionEx<CommonIdName>();
            CloseVisitCommand = new DelegateCommand(CloseVisitAsync, CanSaveChanges);
            CancelCommand = new DelegateCommand(Cancel);
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            reloadVisitDataCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => LoadVisitDataAsync(visitId)) };
            saveChangesCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => CloseVisitAsync()) };
        }
        #endregion

        #region Properties

        private ISuggestionProvider mkbSuggestionProvider;

        [Dependency(SuggestionProviderNames.MKB)]
        public ISuggestionProvider MKBSuggestionProvider
        {
            get { return mkbSuggestionProvider; }
            set { SetProperty(ref mkbSuggestionProvider, value); }
        }

        private DateTime date = DateTime.Now;
        public DateTime Date
        {
            get { return date; }
            set { SetTrackedProperty(ref date, value); }
        }

        public ObservableCollectionEx<CommonIdName> VisitResults { get; set; }

        private int selectedVisitResultId;
        public int SelectedVisitResultId
        {
            get { return selectedVisitResultId; }
            set { SetTrackedProperty(ref selectedVisitResultId, value); }
        }

        public ObservableCollectionEx<CommonIdName> VisitOutcomes { get; set; }

        private int selectedVisitOutcomeId;
        public int SelectedVisitOutcomeId
        {
            get { return selectedVisitOutcomeId; }
            set { SetTrackedProperty(ref selectedVisitOutcomeId, value); }
        }

        private MKB mkb;
        public MKB MKB
        {
            get { return mkb; }
            set { SetTrackedProperty(ref mkb, value); }
        }

        public BusyMediator BusyMediator { get; set; }

        public FailureMediator FailureMediator { get; private set; }

        public IChangeTracker ChangeTracker { get; private set; }
        #endregion

        #region Commands
        public ICommand CloseVisitCommand { get; private set; }
        private async void CloseVisitAsync()
        {
            FailureMediator.Deactivate();
            if (!IsValid)
            {
                return;
            }
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            string visitIdString = visitId < 1 ? visitId.ToString() : "(new visit)";
            logService.InfoFormat("Saving data while closing visit for visit with Id = {0}", visitIdString);
            BusyMediator.Activate("Сохранение изменений...");
            var saveSuccesfull = false;
            try
            {
                var result = await patientRecordsService.CloseVisitAsync(visitId, Date, MKB.Code, SelectedVisitOutcomeId, SelectedVisitResultId, token);
                saveSuccesfull = true;
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to save data while closing visit for visit with Id = {0}", visitIdString);
                FailureMediator.Activate("Не удалось сохранить данные случая. Попробуйте еще раз или обратитесь в службу поддержки", saveChangesCommandWrapper, ex);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {
                    ChangeTracker.AcceptChanges();
                    ChangeTracker.IsEnabled = true;
                    //changeTracker.UntrackAll();
                    HostWindow.Close();
                }
            }
        }

        public ICommand CancelCommand { get; private set; }
        private void Cancel()
        {
            FailureMediator.Deactivate();
            ChangeTracker.RestoreChanges();
            visitId = -1;
            HostWindow.Close();
        }
        #endregion

        #region Methods

        public void IntializeCreation(int visitId, string title)
        {
            this.Title = title;
            BusyMediator.Deactivate();
            FailureMediator.Deactivate();
            FailureMediator = new FailureMediator();
            this.visitId = visitId;
            LoadVisitDataAsync(this.visitId);
        }

        void IDisposable.Dispose()
        {
            ChangeTracker.Dispose();
            reloadVisitDataCommandWrapper.Dispose();
            saveChangesCommandWrapper.Dispose();
        }

        private void OnChangesTracked(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName) || string.CompareOrdinal(e.PropertyName, "HasChanges") == 0)
            {
                (CloseVisitCommand as DelegateCommand).RaiseCanExecuteChanged();
            }
        }
        private bool CanSaveChanges()
        {
            return ChangeTracker.HasChanges;
        }


        private async void LoadVisitDataAsync(int visitId)
        {
            if (visitId < 1)
                return;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            var loadingIsCompleted = false;
            ChangeTracker.IsEnabled = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Заполнение данных закрытия случая...");
            logService.InfoFormat("Loading data from closing visit with id {0}...", visitId);
            IDisposableQueryable<Visit> visitQuery = null;
            IDisposableQueryable<VisitResult> visitResultsQuery = null;
            IDisposableQueryable<VisitOutcome> visitOutcomesQuery = null;
            DateTime curDate = DateTime.Now;
            try
            {
                visitQuery = patientRecordsService.GetVisit(visitId);
                var visit = await visitQuery.FirstOrDefaultAsync(token);
                visitResultsQuery = patientRecordsService.GetActualVisitResults(visit.ExecutionPlaceId, curDate);
                var visitResultsTask = visitResultsQuery.Select(x => new CommonIdName() { Id = x.Id, Name = x.Name }).ToListAsync(token);
                visitOutcomesQuery = patientRecordsService.GetActualVisitOutcomes(visit.ExecutionPlaceId, curDate);
                var visitOutcomesTask = visitOutcomesQuery.Select(x => new CommonIdName() { Id = x.Id, Name = x.Name }).ToListAsync(token);
                await Task.WhenAll(visitResultsTask, visitOutcomesTask);
                VisitOutcomes.AddRange(visitOutcomesTask.Result);
                VisitResults.AddRange(visitResultsTask.Result);
                MKB = patientRecordsService.GetMKB(visit.MKB);
                Date = visit.BeginDateTime;
                SelectedVisitOutcomeId = visit.VisitOutcomeId.ToInt();
                SelectedVisitResultId = visit.VisitResultId.ToInt();
                ChangeTracker.IsEnabled = true;
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data from closing visit with Id {0}", visitId);
                FailureMediator.Activate("Не удалость загрузить случай. Попробуйте еще раз или обратитесь в службу поддержки", reloadVisitDataCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (visitQuery != null)
                {
                    visitQuery.Dispose();
                }
            }
        }
        #endregion

        #region IPopupWindowActionAware implementation
        public System.Windows.Window HostWindow { get; set; }

        public INotification HostNotification { get; set; }
        #endregion

        #region INotification implementation
        public object Content { get; set; }

        public string Title { get; set; }
        #endregion

        #region IDataErrorInfo implementation
        private bool saveWasRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        private bool IsValid
        {
            get
            {
                saveWasRequested = true;
                OnPropertyChanged(string.Empty);
                return invalidProperties.Count < 1;
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                switch (columnName)
                {
                    case "selectedVisitOutcomeId":
                        result = selectedVisitOutcomeId == null ? "Не указан исход" : string.Empty;
                        break;
                    case "selectedVisitResultId":
                        result = selectedVisitResultId == null ? "Не указан результат" : string.Empty;
                        break;
                    case "MKB":
                        result = MKB == null ? "Не указан диагноз" : string.Empty;
                        break;
                }
                if (string.IsNullOrEmpty(result))
                {
                    invalidProperties.Remove(columnName);
                }
                else
                {
                    invalidProperties.Add(columnName);
                }
                return result;
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
