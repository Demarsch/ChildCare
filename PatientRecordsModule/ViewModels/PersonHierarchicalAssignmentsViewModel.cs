using System;
using Core;
using System.Linq;
using System.Collections.ObjectModel;
using Core.Wpf.Mvvm;
using Prism.Mvvm;
using PatientRecordsModule.Services;
using PatientRecordsModule.DTO;

namespace PatientRecordsModule.ViewModels
{
    public class PersonHierarchicalAssignmentsViewModel : BindableBase
    {
        private readonly AssignmentDTO assignment;

        private readonly IPatientRecordsService patientRecordsService;

        public PersonHierarchicalAssignmentsViewModel(AssignmentDTO assignment, IPatientRecordsService patientRecordsService)
        {
            if (assignment == null)
            {
                throw new ArgumentNullException("assignment");
            }
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            this.patientRecordsService = patientRecordsService;
            this.assignment = assignment;
        }

        public int Id { get { return assignment.Id; } }

        public DateTime AssignDateTime { get { return assignment.ActualDateTime; } }

        public string RecordTypeName { get { return assignment.RecordTypeName; } }

        public string RoomName { get { return assignment.RoomName; } }

        public string FinSource { get { return assignment.FinancingSourceName; } }


        private ObservableCollectionEx<object> nestedItems;
        public ObservableCollectionEx<object> NestedItems
        {
            get
            {
                if (nestedItems == null)
                {
                    var childrenList = new ObservableCollectionEx<object>();
                    //childrenList.AddRange(assignmentService.GetChildAssignments(assignment.Id).Select(x => new PersonHierarchicalAssignmentsViewModel(x, assignmentService)));
                    SetProperty(ref nestedItems, childrenList);
                }
                return nestedItems;
            }
        }
    }
}
