using System;
using Core;
using DataLib;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using Core.PersonVisitItemsListViewModels;
using System.Linq;

namespace MainLib.PersonVisitItemsListViewModels
{
    public class PersonHierarchicalRecordsViewModel : ObservableObject
    {
        private readonly RecordDTO record;

        private readonly IRecordService recordService;
        private readonly IAssignmentService assignmentService;

        public PersonHierarchicalRecordsViewModel(RecordDTO recordDTO, IRecordService recordService, IAssignmentService assignmentService)
        {
            if (recordDTO == null)
            {
                throw new ArgumentNullException("recordDTO");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            if (assignmentService == null)
            {
                throw new ArgumentNullException("assignmentService");
            }
            this.assignmentService = assignmentService;
            this.recordService = recordService;
            this.record = recordDTO;
        }

        public int Id { get { return record.Id; } }

        public string DateTimePeriod { get { return record.BeginDateTime.ToString("dd.MM.yyyy") + " - " + (record.EndDateTime.HasValue ? record.EndDateTime.Value.ToString("dd.MM.yyyy") : "..."); } }

        public string RecordTypeName { get { return record.RecordTypeName; } }

        public string RoomName { get { return record.RoomName; } }

        private ObservalbeCollectionEx<object> nestedItems;
        public ObservalbeCollectionEx<object> NestedItems
        {
            get
            {
                if (nestedItems == null)
                {
                    var childrenList = new ObservalbeCollectionEx<object>();
                    childrenList.AddRange(recordService.GetChildRecords(record.Id).Select(x => new PersonHierarchicalRecordsViewModel(x, recordService, assignmentService)));
                    childrenList.AddRange(recordService.GetChildAssignments(record.Id).Select(x => new PersonHierarchicalAssignmentsViewModel(x, assignmentService)));
                    Set(() => NestedItems, ref nestedItems, childrenList);
                }
                return nestedItems;
            }
        }
    }
}
