using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace Core.PersonVisitItemsListViewModel
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
            }
        }

        public ObservalbeCollectionEx<object> RootItems { get; set; }

        #endregion

        #region Commands
        #endregion

        #region Methods

        private async void LoadRootItemsAsync()
        {
            var task = Task.Factory.StartNew(LoadRootItems);
            await task;
        }

        private void LoadRootItems()
        {
            RootItems.Clear();
            RootItems.AddRange(personService.GetRootAssignments(PersonId));
            RootItems.AddRange(personService.GetVisits(PersonId));
        }

        #endregion
    }
}
