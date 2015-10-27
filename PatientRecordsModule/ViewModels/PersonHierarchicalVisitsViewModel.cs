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
    public class PersonHierarchicalVisitsViewModel : BindableBase
    {
        private readonly VisitDTO visit;

        private readonly IPatientRecordsService patientRecordsService;

        public PersonHierarchicalVisitsViewModel(VisitDTO visitDTO, IPatientRecordsService patientRecordsService)
        {
            if (visitDTO == null)
            {
                throw new ArgumentNullException("visitDTO");
            }
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            this.patientRecordsService = patientRecordsService;
            this.visit = visitDTO;
        }

        public int Id { get { return visit.Id; } }

        public string DateTimePeriod { get { return visit.BeginDateTime.ToString("dd.MM.yyyy") + " - " + (visit.EndDateTime.HasValue ? visit.EndDateTime.Value.ToString("dd.MM.yyyy") : "..."); } }

        public string Name { get { return visit.Name; } }

        public string FinSource { get { return visit.FinSource; } }

        public bool IsCompleted { get { return visit.IsCompleted == true; } }

        private ObservableCollectionEx<object> nestedItems;
        public ObservableCollectionEx<object> NestedItems
        {
            get
            {
                if (nestedItems == null)
                {
                    var childrenList = new ObservableCollectionEx<object>();
                    //childrenList.AddRange(visitService.GetChildRecords(visit.Id).Select(x => new PersonHierarchicalRecordsViewModel(x, recordService, assignmentService)));
                    //childrenList.AddRange(visitService.GetChildAssignments(visit.Id).Select(x => new PersonHierarchicalAssignmentsViewModel(x, assignmentService)));
                    SetProperty(ref nestedItems, childrenList);
                }
                return nestedItems;
            }
        }
    }
}
