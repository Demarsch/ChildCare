using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Core;
using Prism.Mvvm;
using Core.Wpf.Mvvm;
using PatientRecordsModule.Services;
using PatientRecordsModule.DTO;

namespace PatientRecordsModule.ViewModels
{
    public class PersonVisitItemsListViewModel : BindableBase
    {
        #region Fields

        private readonly IPatientRecordsService patientRecordsService;

        #endregion

        #region  Constructors
        public PersonVisitItemsListViewModel(int personId, IPatientRecordsService patientRecordsService)
        {
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            this.patientRecordsService = patientRecordsService;
            RootItems = new ObservableCollectionEx<object>();
            this.PersonId = personId;
        }
        #endregion

        #region Properties

        private int personId;
        public int PersonId
        {
            get { return personId; }
            set
            {
                SetProperty(ref personId, value);
                LoadRootItemsAsync();
                LoadRootItemsAsync();
                //RootItems.Clear();
                //RootItems.AddRange(LoadRootItems());
            }
        }

        public ObservableCollectionEx<object> RootItems { get; set; }

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { SetProperty(ref isLoading, value); }
        }

        private string ambNumber;
        public string AmbNumber
        {
            get { return ambNumber; }
            set { SetProperty(ref ambNumber, value); }
        }

        #endregion

        #region Commands
        #endregion

        #region Methods

        private async void LoadRootItemsAsync()
        {
            RootItems.Clear();
            var task = Task<List<object>>.Factory.StartNew(LoadRootItems);
            IsLoading = true;
            await task;
            IsLoading = false;
            RootItems.AddRange(task.Result);
        }

        private List<object> LoadRootItems()
        {
            List<object> resList = new List<object>();
            var assignmentsViewModels = patientRecordsService.GetPersonRootAssignmentsQuery(PersonId)
                .Select(x => new AssignmentDTO()
                {
                    Id = x.Id,
                    ActualDateTime = x.AssignDateTime,
                    FinancingSourceName = x.FinancingSource.ShortName,
                    RecordTypeName = x.RecordType.Name,
                    RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name
                })
                .ToArray()
                .Select(x => new PersonHierarchicalAssignmentsViewModel(x, patientRecordsService));
            var visitsViewModels = patientRecordsService.GetPersonVisitsQuery(PersonId)
                .Select(x => new VisitDTO()
                {
                    Id = x.Id,
                    BeginDateTime = x.BeginDateTime,
                    EndDateTime = x.EndDateTime,
                    ActualDateTime = x.BeginDateTime,
                    FinSource = x.FinancingSource.ShortName,
                    Name = x.VisitTypeId.ToString(),
                    IsCompleted = x.IsCompleted,
                })
                .ToArray()
                .Select(x => new PersonHierarchicalVisitsViewModel(x, patientRecordsService));
            resList.AddRange(assignmentsViewModels);
            resList.AddRange(visitsViewModels);
            return resList.OrderBy(x => ((dynamic)x).ActualDateTime).ToList();
        }

        #endregion
    }
}
