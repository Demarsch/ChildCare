using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using Core;

namespace MainLib.PersonVisitItemsListViewModels
{
    public class PersonVisitItemsListViewModel : ObservableObject
    {
        #region Fields

        private readonly IPersonService personService;
        private readonly IVisitService visitService;
        private readonly IAssignmentService assignmentService;
        private readonly IRecordService recordService;

        #endregion

        #region  Constructors
        public PersonVisitItemsListViewModel(int personId, IPersonService personService, IVisitService visitService, IAssignmentService assignmentService, IRecordService recordService)
        {
            if (personService == null)
            {
                throw new ArgumentNullException("personService");
            }
            if (visitService == null)
            {
                throw new ArgumentNullException("visitService");
            }
            if (assignmentService == null)
            {
                throw new ArgumentNullException("assignmentService");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            this.personService = personService;
            this.visitService = visitService;
            this.assignmentService = assignmentService;
            this.recordService = recordService;
            RootItems = new ObservalbeCollectionEx<object>();
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
                Set(() => PersonId, ref personId, value);
                LoadRootItemsAsync();
                //RootItems.Clear();
                //RootItems.AddRange(LoadRootItems());
            }
        }

        public ObservalbeCollectionEx<object> RootItems { get; set; }

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { Set(() => IsLoading, ref isLoading, value); }
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
            var assignmentsViewModels = personService.GetRootAssignments(PersonId).Select(x => new PersonHierarchicalAssignmentsViewModel(x, assignmentService));
            var visitsViewModels = personService.GetVisits(PersonId).Select(x => new PersonHierarchicalVisitsViewModel(x, visitService, recordService, assignmentService));
            resList.AddRange(assignmentsViewModels);
            resList.AddRange(visitsViewModels);
            return resList;
        }

        #endregion
    }
}
