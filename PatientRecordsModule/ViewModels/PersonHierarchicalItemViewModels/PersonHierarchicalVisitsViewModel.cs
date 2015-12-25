using System;
using Core;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using Shared.PatientRecords.Services;
using Core.Wpf.Mvvm;
using Shared.PatientRecords.DTO;
using System.Threading.Tasks;
using System.Collections.Generic;
using log4net;
using Core.Extensions;
using Core.Wpf.Misc;
using Prism.Commands;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using System.Threading;
using System.Data.Entity;
using Core.Misc;
using Core.Wpf.Events;
using Prism.Events;
using Shared.PatientRecords.Misc;
using Shared.PatientRecords.ViewModels.PersonHierarchicalItemViewModels;

namespace Shared.PatientRecords.ViewModels
{
    public class PersonHierarchicalVisitsViewModel : BindableBase, IDisposable, IHierarchicalItem
    {
        #region Fields

        private readonly IPatientRecordsService patientRecordsService;
        private readonly IEventAggregator eventAggregator;
        private readonly ILog logService;
        private readonly IHierarchicalRepository childItemViewModelRepository;

        private readonly CommandWrapper reloadPatientVisitsCommandWrapper;
        private readonly CommandWrapper initializetAssignmentCommandWrapper;

        private CancellationTokenSource currentLoadingToken;
        #endregion

        #region Constructors
        public PersonHierarchicalVisitsViewModel(VisitDTO visitDTO, IPatientRecordsService patientRecordsService, IEventAggregator eventAggregator, ILog logService, IHierarchicalRepository childItemViewModelRepository)
        {
            if (childItemViewModelRepository == null)
            {
                throw new ArgumentNullException("childItemViewModelRepository");
            }
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            this.eventAggregator = eventAggregator;
            this.logService = logService;
            this.patientRecordsService = patientRecordsService;
            this.childItemViewModelRepository = childItemViewModelRepository;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            reloadPatientVisitsCommandWrapper = new CommandWrapper
            {
                Command = new DelegateCommand(() => LoadItemsAsync()),
                CommandName = "Повторить",
            };
            initializetAssignmentCommandWrapper = new CommandWrapper
            {
                Command = new DelegateCommand<PersonRecItem>((x) => Initialize(x)),
                CommandParameter = Item,
                CommandName = "Повторить",
            };
        }
        #endregion

        #region Properties
        //visit.BeginDateTime.ToString("dd.MM.yyyy") + " - " + (visit.EndDateTime.HasValue ? visit.EndDateTime.Value.ToString("dd.MM.yyyy") : "..."); 
        private DateTime actualDateTime;
        public DateTime ActualDateTime
        {
            get { return actualDateTime; }
            set { SetProperty(ref actualDateTime, value); }
        }

        private string dateTimePeriod = string.Empty;
        public string DateTimePeriod
        {
            get { return dateTimePeriod; }
            set { SetProperty(ref dateTimePeriod, value); }
        }

        private string name = string.Empty;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private string finSource = string.Empty;
        public string FinSource
        {
            get { return finSource; }
            set { SetProperty(ref finSource, value); }
        }

        private bool isCompleted = false;
        public bool IsCompleted
        {
            get { return isCompleted; }
            set { SetProperty(ref isCompleted, value); }
        }

        private ObservableCollectionEx<IHierarchicalItem> childs;
        public ObservableCollectionEx<IHierarchicalItem> Childs
        {
            get
            {
                if (childs == null)
                    LoadItemsAsync();
                return childs;
            }
            set { SetProperty(ref childs, value); }
        }

        private PersonRecItem item;
        public PersonRecItem Item
        {
            get { return item; }
            set { SetProperty(ref item, value); }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                SetProperty(ref isSelected, value);
                //if (SetProperty(ref isSelected, value) && value)
                //{
                //    eventAggregator.GetEvent<SelectionChangedEvent<Visit>>().Publish(this.Id);
                //}
            }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetProperty(ref isExpanded, value); }
        }

        public IPersonRecordEditor PersonRecordEditorViewModel { get; private set; }

        public FailureMediator FailureMediator { get; private set; }

        public BusyMediator BusyMediator { get; set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            reloadPatientVisitsCommandWrapper.Dispose();
            initializetAssignmentCommandWrapper.Dispose();
        }

        public async void Initialize(PersonRecItem item)
        {
            childs = null;
            Item = item;
            ActualDateTime = SpecialValues.MinDate;
            FinSource = string.Empty;
            Name = string.Empty;

            var loadingIsCompleted = false;
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate(string.Empty);
            logService.InfoFormat("Loading visit hierarchical item for visit with Id {0}...", Item.Id);
            IDisposableQueryable<Visit> visitQuery = null;
            try
            {
                visitQuery = patientRecordsService.GetVisit(Item.Id);
                var visit = await visitQuery.Select(x => new
                {
                    x.BeginDateTime,
                    x.EndDateTime,
                    ActualDateTime = x.BeginDateTime,
                    FinancingSourceName = x.FinancingSource.Name,
                    Name = x.VisitTemplate.Name,
                }).FirstOrDefaultAsync(token);

                ActualDateTime = visit.ActualDateTime;
                DateTimePeriod = visit.BeginDateTime.ToString("dd.MM.yyyy") + " - " + (visit.EndDateTime.HasValue ? visit.EndDateTime.Value.ToString("dd.MM.yyyy") : "...");
                FinSource = visit.FinancingSourceName;
                Name = visit.Name;

                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load visit hierarchical item for visit with Id {0}", Item.Id);
                FailureMediator.Activate("Не удалость загрузить данные случая. Попробуйте еще раз или обратитесь в службу поддержки", initializetAssignmentCommandWrapper, ex, true);
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

        private async void LoadItemsAsync()
        {
            var loadingIsCompleted = false;
            if (childs == null)
                Childs = new ObservableCollectionEx<IHierarchicalItem>();
            Childs.Clear();
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate(string.Empty);
            logService.InfoFormat("Loading child items for visit with Id {0}...", Item.Id);
            IDisposableQueryable<Assignment> childAssignmentsQuery = null;
            IDisposableQueryable<Record> childRecordsQuery = null;
            try
            {
                childAssignmentsQuery = patientRecordsService.GetVisitsChildAssignmentsQuery(Item.Id);
                childRecordsQuery = patientRecordsService.GetVisitsChildRecordsQuery(Item.Id);
                var loadChildAssignmentsTask = childAssignmentsQuery.Select(x => new PersonRecItem
                    {
                        Id = x.Id,
                        ActualDatetime = x.AssignDateTime,
                        Type = ItemType.Assignment
                    }).ToListAsync(token);
                var loadChildRecordsTask = childRecordsQuery.Select(x => new PersonRecItem
                {
                    Id = x.Id,
                    ActualDatetime = x.ActualDateTime,
                    Type = ItemType.Record
                }).ToListAsync(token);
                await Task.WhenAll(loadChildAssignmentsTask, loadChildRecordsTask);
                var resChilds = loadChildAssignmentsTask.Result.Union(loadChildRecordsTask.Result).OrderBy(x => x.ActualDatetime);
                Childs.AddRange(resChilds.Select(x => childItemViewModelRepository.GetHierarchicalItem(x)));
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load child items for visit with Id {0}", Item.Id);
                FailureMediator.Activate("Не удалость загрузить вложенные элементы случая. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientVisitsCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (childAssignmentsQuery != null)
                {
                    childAssignmentsQuery.Dispose();
                }
                if (childRecordsQuery != null)
                {
                    childRecordsQuery.Dispose();
                }
            }
            return;
        }
        #endregion
    }
}
