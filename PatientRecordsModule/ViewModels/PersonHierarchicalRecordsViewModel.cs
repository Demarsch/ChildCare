﻿using System;
using Core;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using PatientRecordsModule.Services;
using Core.Wpf.Mvvm;
using PatientRecordsModule.DTO;
using log4net;
using Core.Wpf.Misc;
using System.Threading;
using Prism.Commands;
using Core.Data.Misc;
using Core.Data;
using System.Data.Entity;
using System.Threading.Tasks;
using Core.Misc;
using Core.Extensions;
using System.Windows.Input;

namespace PatientRecordsModule.ViewModels
{
    public class PersonHierarchicalRecordsViewModel : BindableBase
    {
        #region Fields
        private readonly RecordDTO record;

        private readonly IPatientRecordsService patientRecordsService;

        private readonly ILog logService;

        private readonly CommandWrapper reloadPatientVisitsCommandWrapper;

        private CancellationTokenSource currentLoadingToken;
        #endregion

        #region Constructors
        public PersonHierarchicalRecordsViewModel(RecordDTO recordDTO, IPatientRecordsService patientRecordsService, ILog logService)
        {
            if (recordDTO == null)
            {
                throw new ArgumentNullException("recordDTO");
            }
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            this.logService = logService;
            this.patientRecordsService = patientRecordsService;
            this.record = recordDTO;
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            reloadPatientVisitsCommandWrapper = new CommandWrapper
            {
                Command = new DelegateCommand(() => LoadItemsAsync()),
                CommandName = "Повторить",
            };
        }
        #endregion

        #region Properties
        public int Id { get { return record.Id; } }

        public string DateTimePeriod { get { return record.BeginDateTime.ToString("dd.MM.yyyy") + " - " + (record.EndDateTime.HasValue ? record.EndDateTime.Value.ToString("dd.MM.yyyy") : "..."); } }

        public string RecordTypeName { get { return record.RecordTypeName; } }

        public string FinSource { get { return record.FinSourceName; } }

        public string RoomName { get { return record.RoomName; } }

        public bool IsCompleted { get { return record.IsCompleted == true; } }

        private ObservableCollectionEx<object> nestedItems;
        public ObservableCollectionEx<object> NestedItems
        {
            get
            {
                if (nestedItems == null)
                    LoadItemsAsync();
                return nestedItems;
            }
            set { SetProperty(ref nestedItems, value); }
        }

        public CriticalFailureMediator CriticalFailureMediator { get; private set; }

        public BusyMediator BusyMediator { get; set; }

        #endregion

        #region Methods
        private async void LoadItemsAsync()
        {
            var loadingIsCompleted = false;
            if (nestedItems == null)
                NestedItems = new ObservableCollectionEx<object>();
            NestedItems.Clear();
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate(string.Empty);
            logService.InfoFormat("Loading child items for record with Id {0}...", record.Id);
            IDisposableQueryable<Assignment> childAssignmentsQuery = null;
            IDisposableQueryable<Record> childRecordsQuery = null;
            try
            {
                childAssignmentsQuery = patientRecordsService.GetRecordsChildAssignmentsQuery(record.Id);
                childRecordsQuery = patientRecordsService.GetRecordsChildRecordsQuery(record.Id);
                var loadChildAssignmentsTask = childAssignmentsQuery.Select(x => new AssignmentDTO()
                {
                    Id = x.Id,
                    ActualDateTime = x.AssignDateTime,
                    FinancingSourceName = x.FinancingSource.ShortName,
                    RecordTypeName = x.RecordType.Name,
                    RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name,
                }).ToListAsync(token);
                var loadChildRecordsTask = childRecordsQuery.Select(x => new RecordDTO()
                {
                    Id = x.Id,
                    ActualDateTime = x.ActualDateTime,
                    RecordTypeName = x.RecordType.Name,
                    BeginDateTime = x.BeginDateTime,
                    EndDateTime = x.EndDateTime,
                    IsCompleted = x.IsCompleted,
                    FinSourceName = "ист. фин.",
                    RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name,
                }).ToListAsync(token);
                await Task.WhenAll(loadChildAssignmentsTask, loadChildRecordsTask, Task.Delay(AppConfiguration.PendingOperationDelay, token));
                NestedItems.AddRange(loadChildAssignmentsTask.Result.Select(x => new PersonHierarchicalAssignmentsViewModel(x, patientRecordsService, logService)));
                NestedItems.AddRange(loadChildRecordsTask.Result.Select(x => new PersonHierarchicalRecordsViewModel(x, patientRecordsService, logService)));
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to child items for record with Id {0}", record.Id);
                CriticalFailureMediator.Activate("Не удалость загрузить вложенные элементы услуги. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientVisitsCommandWrapper, ex);
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
