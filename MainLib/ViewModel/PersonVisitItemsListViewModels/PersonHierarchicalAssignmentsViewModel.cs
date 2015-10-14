using System;
using Core;
using DataLib;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace MainLib.PersonVisitItemsListViewModels
{
    public class PersonHierarchicalAssignmentsViewModel : ObservableObject
    {
        private readonly AssignmentDTO assignment;

        private readonly IPersonService personService;

        public PersonHierarchicalAssignmentsViewModel(AssignmentDTO assignment, IPersonService personService)
        {
            if (assignment == null)
            {
                throw new ArgumentNullException("assignment");
            }
            if (personService == null)
            {
                throw new ArgumentNullException("personService");
            }
            this.personService = personService;
            this.assignment = assignment;
        }

        public int Id { get { return assignment.Id; } }

        public DateTime AssignDateTime { get { return assignment.AssignDateTime; } }

        public string RecordTypeName { get { return assignment.RecordTypeName; } }

        public string RoomName { get { return assignment.RoomName; } }

        public ObservableCollection<object> Childern
        {
            get
            {
                var childrenList = new ObservableCollection<object>();
                //childrenList.AddRange(personService.GetChildAssignments(assignment.Id));
                return childrenList;
            }
        }
    }
}
