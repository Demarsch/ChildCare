using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Navigation;
using Core.Data;
using Core.Data.Classes;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;
using StatisticsModule.Services;
using Prism.Regions;
using System.Windows.Input;

namespace StatisticsModule.ViewModels
{
    public class RecordsStatisticsViewModel: BindableBase, IDisposable, INavigationAware
    {
        private readonly IStatisticsService statisticsService;

        private readonly ILog logService;

        private readonly IDialogServiceAsync dialogService;

        private readonly IDialogService messageService;

        public BusyMediator BusyMediator { get; set; }

        public RecordsStatisticsViewModel(IStatisticsService statisticsService,
                                      IDialogServiceAsync dialogService,
                                      IDialogService messageService,
                                      ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (statisticsService == null)
            {
                throw new ArgumentNullException("statisticsService");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }

            this.statisticsService = statisticsService;
            this.logService = logService;
            this.dialogService = dialogService;
            this.messageService = messageService;
            loadResultCommand = new DelegateCommand(LoadResult);

            BusyMediator = new BusyMediator();
            FinSources = new ObservableCollectionEx<FieldValue>();
        }

        internal async Task InitialLoadingAsync()
        {
            FinSources.Clear();            
            BusyMediator.Activate("Загрузка данных...");
            logService.Info("Loading data sources...");
            IDisposableQueryable<FinancingSource> finSourcesQuery = null;
            try
            {               
                finSourcesQuery = statisticsService.GetActualFinancingSources();
                var finSourcesSelectQuery = await finSourcesQuery.Where(x => x.Options != string.Empty).Select(x => new { x.Id, x.Name }).ToArrayAsync();
                FinSources.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- все ист. финансирования -" });
                FinSources.AddRange(finSourcesSelectQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
                SelectedFinSourceId = SpecialValues.NonExistingId;

                BeginDate = DateTime.Now.Date;
                EndDate = new DateTime(BeginDate.Year + 1, 1, 1);

                logService.InfoFormat("Data sources are successfully loaded");
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources");
                messageService.ShowError("Не удалось загрузить данные. ");
            }
            finally
            {
                if (finSourcesQuery != null)
                    finSourcesQuery.Dispose();
                BusyMediator.Deactivate();
            }
        }

        private void LoadResult()
        {

        }

        #region Properties

        private readonly DelegateCommand loadResultCommand;
        public ICommand LoadResultCommand { get { return loadResultCommand; } }

        private ObservableCollectionEx<FieldValue> finSources;
        public ObservableCollectionEx<FieldValue> FinSources
        {
            get { return finSources; }
            set { SetProperty(ref finSources, value); }
        }

        private int selectedFinSourceId;
        public int SelectedFinSourceId
        {
            get { return selectedFinSourceId; }
            set { SetProperty(ref selectedFinSourceId, value); }
        }

        private DateTime beginDate;
        public DateTime BeginDate
        {
            get { return beginDate; }
            set { SetProperty(ref beginDate, value); }
        }

        private DateTime endDate;
        public DateTime EndDate
        {
            get { return endDate; }
            set { SetProperty(ref endDate, value); }
        }

        #endregion

        #region

        public void Dispose()
        {
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await InitialLoadingAsync();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        #endregion
    }
}
