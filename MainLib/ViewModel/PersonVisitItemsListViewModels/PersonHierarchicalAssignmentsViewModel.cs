using System;
using Core;
using DataLib;
using GalaSoft.MvvmLight;
using System.Linq;
using System.Collections.ObjectModel;

namespace MainLib.PersonVisitItemsListViewModels
{
    public class PersonHierarchicalAssignmentsViewModel : ObservableObject
    {
        private readonly AssignmentDTO assignment;

        private readonly IAssignmentService assignmentService;

        public PersonHierarchicalAssignmentsViewModel(AssignmentDTO assignment, IAssignmentService assignmentService)
        {
            if (assignment == null)
            {
                throw new ArgumentNullException("assignment");
            }
            if (assignmentService == null)
            {
                throw new ArgumentNullException("assignmentService");
            }
            this.assignmentService = assignmentService;
            this.assignment = assignment;
        }

        public int Id { get { return assignment.Id; } }

        public DateTime AssignDateTime { get { return assignment.AssignDateTime; } }

        public string RecordTypeName { get { return assignment.RecordTypeName; } }

        public string RoomName { get { return assignment.RoomName; } }


        private ObservalbeCollectionEx<object> nestedItems;
        public ObservalbeCollectionEx<object> NestedItems
        {
            get
            {
                if (nestedItems != null)
                {
                    var childrenList = new ObservalbeCollectionEx<object>();
                    childrenList.AddRange(assignmentService.GetChildAssignments(assignment.Id).Select(x => new PersonHierarchicalAssignmentsViewModel(x, assignmentService)));
                    Set(() => NestedItems, ref nestedItems, childrenList);
                }
                return nestedItems;
            }
        }
    }
}
