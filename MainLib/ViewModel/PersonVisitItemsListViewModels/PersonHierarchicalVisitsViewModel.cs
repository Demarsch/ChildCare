using System;
using Core;
using DataLib;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using Core.PersonVisitItemsListViewModels;
using System.Linq;

namespace MainLib.PersonVisitItemsListViewModels
{
    public class PersonHierarchicalVisitsViewModel : ObservableObject
    {
        private readonly VisitDTO visit;

        private readonly IVisitService visitService;
        private readonly IRecordService recordService;
        private readonly IAssignmentService assignmentService;

        public PersonHierarchicalVisitsViewModel(VisitDTO visitDTO, IVisitService visitService, IRecordService recordService, IAssignmentService assignmentService)
        {
            if (visitDTO == null)
            {
                throw new ArgumentNullException("visitDTO");
            }
            if (visitService == null)
            {
                throw new ArgumentNullException("visitService");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            if (assignmentService == null)
            {
                throw new ArgumentNullException("assignmentService");
            }
            this.visitService = visitService;
            this.recordService = recordService;
            this.assignmentService = assignmentService;
            this.visit = visitDTO;
        }

        public int Id { get { return visit.Id; } }

        public string DateTimePeriod { get { return visit.BeginDateTime.ToString("dd.MM.yyyy") + " - " + (visit.EndDateTime.HasValue ? visit.EndDateTime.Value.ToString("dd.MM.yyyy") : "..."); } }

        public string Name { get { return visit.Name; } }

        private ObservalbeCollectionEx<object> nestedItems;
        public ObservalbeCollectionEx<object> NestedItems
        {
            get
            {
                if (nestedItems == null)
                {
                    var childrenList = new ObservalbeCollectionEx<object>();
                    childrenList.AddRange(visitService.GetChildRecords(visit.Id).Select(x => new PersonHierarchicalRecordsViewModel(x, recordService, assignmentService)));
                    childrenList.AddRange(visitService.GetChildAssignments(visit.Id).Select(x => new PersonHierarchicalAssignmentsViewModel(x, assignmentService)));
                    Set(() => NestedItems, ref nestedItems, childrenList);
                }
                return nestedItems;
            }
        }
    }
}
