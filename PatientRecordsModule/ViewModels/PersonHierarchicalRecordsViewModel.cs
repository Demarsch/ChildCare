using System;
using Core;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using PatientRecordsModule.Services;
using Core.Wpf.Mvvm;
using PatientRecordsModule.DTO;

namespace PatientRecordsModule.ViewModels
{
    public class PersonHierarchicalRecordsViewModel : BindableBase
    {
        private readonly RecordDTO record;

        private readonly IPatientRecordsService patientRecordsService;

        public PersonHierarchicalRecordsViewModel(RecordDTO recordDTO, IPatientRecordsService patientRecordsService)
        {
            if (recordDTO == null)
            {
                throw new ArgumentNullException("recordDTO");
            }
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            this.patientRecordsService = patientRecordsService;
            this.record = recordDTO;
        }

        public int Id { get { return record.Id; } }

        public string DateTimePeriod { get { return record.BeginDateTime.ToString("dd.MM.yyyy") + " - " + (record.EndDateTime.HasValue ? record.EndDateTime.Value.ToString("dd.MM.yyyy") : "..."); } }

        public string RecordTypeName { get { return record.RecordTypeName; } }

        public string RoomName { get { return record.RoomName; } }

        public bool IsCompleted { get { return record.IsCompleted == true; } }

        private ObservableCollectionEx<object> nestedItems;
        public ObservableCollectionEx<object> NestedItems
        {
            get
            {
                if (nestedItems == null)
                {
                    var childrenList = new ObservableCollectionEx<object>();
                    //childrenList.AddRange(recordService.GetChildRecords(record.Id).Select(x => new PersonHierarchicalRecordsViewModel(x, recordService, assignmentService)));
                    //childrenList.AddRange(recordService.GetChildAssignments(record.Id).Select(x => new PersonHierarchicalAssignmentsViewModel(x, assignmentService)));
                    SetProperty(ref nestedItems, childrenList);
                }
                return nestedItems;
            }
        }
    }
}
