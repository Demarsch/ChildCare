using Shared.PatientRecords.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.PatientRecords.ViewModels.PersonHierarchicalItemViewModels
{
    public class HierarchicalRepository : IHierarchicalRepository
    {
        private readonly Func<PersonHierarchicalAssignmentsViewModel> personHierarchicalAssignmentsViewModelFactory;
        private readonly Func<PersonHierarchicalRecordsViewModel> personHierarchicalRecordsViewModelFactory;
        private readonly Func<PersonHierarchicalVisitsViewModel> personHierarchicalVisitsViewModelFactory;

        private readonly Func<PersonRecordEditorViewModel> personRecordEditorViewModelFacotory;

        private Dictionary<PersonRecItem, IPersonRecordEditor> editors;

        public HierarchicalRepository(Func<PersonHierarchicalAssignmentsViewModel> personHierarchicalAssignmentsViewModelFactory, Func<PersonHierarchicalRecordsViewModel> personHierarchicalRecordsViewModelFactory,
            Func<PersonHierarchicalVisitsViewModel> personHierarchicalVisitsViewModelFactory, Func<PersonRecordEditorViewModel> personRecordEditorViewModelFacotory)
        {
            if (personHierarchicalAssignmentsViewModelFactory == null)
            {
                throw new ArgumentNullException("personHierarchicalAssignmentsViewModelFactory");
            }
            if (personHierarchicalRecordsViewModelFactory == null)
            {
                throw new ArgumentNullException("personHierarchicalRecordsViewModelFactory");
            }
            if (personHierarchicalVisitsViewModelFactory == null)
            {
                throw new ArgumentNullException("personHierarchicalVisitsViewModelFactory");
            }
            if (personRecordEditorViewModelFacotory == null)
            {
                throw new ArgumentNullException("personRecordEditorViewModelFacotory");
            }
            this.personRecordEditorViewModelFacotory = personRecordEditorViewModelFacotory;
            this.personHierarchicalVisitsViewModelFactory = personHierarchicalVisitsViewModelFactory;
            this.personHierarchicalRecordsViewModelFactory = personHierarchicalRecordsViewModelFactory;
            this.personHierarchicalAssignmentsViewModelFactory = personHierarchicalAssignmentsViewModelFactory;

            editors = new Dictionary<PersonRecItem, IPersonRecordEditor>();
        }

        public IHierarchicalItem GetHierarchicalItem(PersonRecItem personRecItem)
        {
            IHierarchicalItem hierarchicalItem = null;
            switch (personRecItem.Type)
            {
                case ItemType.Visit:
                    hierarchicalItem = personHierarchicalVisitsViewModelFactory();
                    break;
                case ItemType.Record:
                    hierarchicalItem = personHierarchicalRecordsViewModelFactory();
                    break;
                case ItemType.Assignment:
                    hierarchicalItem = personHierarchicalAssignmentsViewModelFactory();
                    break;
                default:
                    return null;
            }
            hierarchicalItem.Initialize(personRecItem);
            return hierarchicalItem;
        }

        public IPersonRecordEditor GetEditor(PersonRecItem personRecItem)
        {
            if (editors.ContainsKey(personRecItem))
                return editors[personRecItem];
            IPersonRecordEditor personRecordEditor = personRecordEditorViewModelFacotory();
            switch (personRecItem.Type)
            {
                case ItemType.Visit:
                    personRecordEditor.SetRVAIds(personRecItem.Id, 0, 0);
                    break;
                case ItemType.Record:
                    personRecordEditor.SetRVAIds(0, 0, personRecItem.Id);
                    break;
                case ItemType.Assignment:
                    personRecordEditor.SetRVAIds(0, personRecItem.Id, 0);
                    break;
                default:
                    return null;
            }
            editors[personRecItem] = personRecordEditor;
            return personRecordEditor;
        }
    }
}
