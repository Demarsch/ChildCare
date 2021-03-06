﻿using System;
using Core;
using System.Linq;
using System.Collections.ObjectModel;
using Core.Wpf.Mvvm;
using Prism.Mvvm;
using Shared.PatientRecords.Services;
using Shared.PatientRecords.DTO;
using log4net;
using Core.Wpf.Misc;
using System.Threading;
using Prism.Commands;
using Core.Data.Misc;
using Core.Data;
using Core.Extensions;
using System.Data.Entity;
using System.Threading.Tasks;
using Core.Misc;
using System.Windows.Input;
using Prism.Events;
using Core.Wpf.Events;
using System.Collections.Generic;
using Shared.PatientRecords.Misc;
using Shared.PatientRecords.ViewModels.PersonHierarchicalItemViewModels;

namespace Shared.PatientRecords.ViewModels
{
    public class PersonHierarchicalAssignmentsViewModel : BindableBase, IDisposable, IHierarchicalItem
    {
        #region Fields

        private readonly IPatientRecordsService patientRecordsService;
        private readonly IEventAggregator eventAggregator;
        private readonly ILog logService;
        private readonly IHierarchicalRepository childItemViewModelRepository;

        private readonly CommandWrapper reloadChildsForAssignmentCommandWrapper;
        private readonly CommandWrapper initializetAssignmentCommandWrapper;


        private CancellationTokenSource currentLoadingToken;
        #endregion

        #region Constructors
        public PersonHierarchicalAssignmentsViewModel(IPatientRecordsService patientRecordsService, IEventAggregator eventAggregator, ILog logService, IHierarchicalRepository childItemViewModelRepository)
        {
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
            if (childItemViewModelRepository == null)
            {
                throw new ArgumentNullException("childItemViewModelRepository");
            }
            this.childItemViewModelRepository = childItemViewModelRepository;
            this.eventAggregator = eventAggregator;
            this.logService = logService;
            this.patientRecordsService = patientRecordsService;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            reloadChildsForAssignmentCommandWrapper = new CommandWrapper
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

        private DateTime actualDateTime;
        public DateTime ActualDateTime
        {
            get { return actualDateTime; }
            set { SetProperty(ref actualDateTime, value); }
        }

        private string recordTypeName = string.Empty;
        public string RecordTypeName
        {
            get { return recordTypeName; }
            set { SetProperty(ref recordTypeName, value); }
        }

        private string roomName = string.Empty;
        public string RoomName
        {
            get { return roomName; }
            set { SetProperty(ref roomName, value); }
        }

        private string finSource = string.Empty;
        public string FinSource
        {
            get { return finSource; }
            set { SetProperty(ref finSource, value); }
        }

        private bool isExpanded = false;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetProperty(ref isExpanded, value); }
        }

        private bool isSelected = false;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (SetProperty(ref isSelected, value) && value)
                {
                    PersonRecordEditorViewModel = childItemViewModelRepository.GetEditor(this.Item);
                    eventAggregator.GetEvent<PubSubEvent<IPersonRecordEditor>>().Publish(PersonRecordEditorViewModel);
                }

                //eventAggregator.GetEvent<SelectionChangedEvent<Record>>().Publish(item.Id); ;
            }
        }

        private PersonRecItem item;
        public PersonRecItem Item
        {
            get { return item; }
            set { SetProperty(ref item, value); }
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

        public IPersonRecordEditor PersonRecordEditorViewModel { get; private set; }

        public FailureMediator FailureMediator { get; private set; }

        public BusyMediator BusyMediator { get; set; }
        #endregion

        #region Methods

        public void Dispose()
        {
            reloadChildsForAssignmentCommandWrapper.Dispose();
            initializetAssignmentCommandWrapper.Dispose();
        }

        public async void Initialize(PersonRecItem item)
        {
            childs = null;
            Item = item;
            ActualDateTime = SpecialValues.MinDate;
            FinSource = string.Empty;
            RecordTypeName = string.Empty;
            RoomName = string.Empty;

            var loadingIsCompleted = false;
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate(string.Empty);
            logService.InfoFormat("Loading assignment hierarchical item for assignment with Id {0}...", Item.Id);
            IDisposableQueryable<Assignment> assignmentQuery = null;
            try
            {
                assignmentQuery = patientRecordsService.GetAssignment(Item.Id);
                var assignment = await assignmentQuery.Select(x => new
                {
                    ActualDateTime = x.AssignDateTime,
                    FinancingSourceName = x.FinancingSource.Name,
                    RecordTypeName = x.RecordType.ShortName != string.Empty ? x.RecordType.ShortName : x.RecordType.Name,
                    RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name,
                }).FirstOrDefaultAsync(token);

                ActualDateTime = assignment.ActualDateTime;
                FinSource = assignment.FinancingSourceName;
                RecordTypeName = assignment.RecordTypeName;
                RoomName = assignment.RoomName;

                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load assignment hierarchical item for assignment with Id {0}", Item.Id);
                FailureMediator.Activate("Не удалость загрузить данные назначения. Попробуйте еще раз или обратитесь в службу поддержки", initializetAssignmentCommandWrapper, ex, true);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (assignmentQuery != null)
                {
                    assignmentQuery.Dispose();
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
            logService.InfoFormat("Loading child items for assignment with Id {0}...", Item.Id);
            IDisposableQueryable<Assignment> childAssignmentsQuery = null;
            IDisposableQueryable<Record> childRecordsQuery = null;
            try
            {
                childAssignmentsQuery = patientRecordsService.GetAssignmentsChildAssignmentsQuery(Item.Id);
                childRecordsQuery = patientRecordsService.GetAssignmentsChildRecordsQuery(Item.Id);
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
                logService.ErrorFormatEx(ex, "Failed to load child items for assignment with Id {0}", Item.Id);
                FailureMediator.Activate("Не удалость загрузить вложенные элементы назначения. Попробуйте еще раз или обратитесь в службу поддержки", reloadChildsForAssignmentCommandWrapper, ex);
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
        }
        #endregion
    }
}
