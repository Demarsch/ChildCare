using Core.Data.Misc;
using Core.Misc;
using Core.Wpf.Misc;
using PatientInfoModule.Services;
using System;
using System.Drawing;
using System.Windows;
using System.Linq;
using Core.Data;

namespace PatientInfoModule.ViewModels
{
    public class ContractItemViewModel : TrackableBindableBase, IChangeTrackerMediator, IDisposable
    {
        private readonly IRecordService recordService;
        private readonly IPatientService personService;
        public int financingSourceId = SpecialValues.NonExistingId;
        public DateTime contractDate = SpecialValues.MinDate;
        public bool isChild = false;

        public ContractItemViewModel(IRecordService recordService, IPatientService personService, int personId, int financingSourceId, DateTime contractDate)
        {
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            if (personService == null)
            {
                throw new ArgumentNullException("personService");
            }
            this.recordService = recordService;
            this.personService = personService;
            this.financingSourceId = financingSourceId;
            this.contractDate = contractDate;
            this.isChild = personService.GetPatientQuery(personId).First<Person>().BirthDate.Date.AddYears(18) >= contractDate.Date;

            CompositeChangeTracker = new ChangeTrackerEx<ContractItemViewModel>(this);
        }

        private int id;
        public int Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private int? recordContractId;
        public int? RecordContractId
        {
            get { return recordContractId; }
            set { SetProperty(ref recordContractId, value); }
        }

        private int? assignmentId;
        public int? AssignmentId
        {
            get { return assignmentId; }
            set { SetProperty(ref assignmentId, value); }
        }

        private int recordTypeId;
        public int RecordTypeId
        {
            get { return recordTypeId; }
            set { SetProperty(ref recordTypeId, value); }
        }

        private bool isPaid;
        public bool IsPaid
        {
            get { return isPaid; }
            set
            {
                SetTrackedProperty(ref isPaid, value);
            }
        }

        private string recordTypeName;
        public string RecordTypeName
        {
            get { return recordTypeName; }
            set { SetProperty(ref recordTypeName, value); }
        }

        private int recordCount;
        public int RecordCount
        {
            get { return recordCount; }
            set 
            {
                if (SetProperty(ref recordCount, value))
                    RecordCost = (recordService.GetRecordTypeCost(recordTypeId, financingSourceId, contractDate, isChild) * recordCount);    
            }
        }

        private double recordCost;
        public double RecordCost
        {
            get { return recordCost; }
            set { SetProperty(ref recordCost, value); }
        }

        private int? appendix;
        public int? Appendix
        {
            get { return appendix; }
            set { SetProperty(ref appendix, value); }
        }

        private bool isSection;
        public bool IsSection
        {
            get { return isSection; }
            set { SetProperty(ref isSection, value); }
        }

        private string sectionName;
        public string SectionName
        {
            get { return sectionName; }
            set { SetProperty(ref sectionName, value); }
        }

        private Color sectionBackColor;
        public Color SectionBackColor
        {
            get { return sectionBackColor; }
            set { SetProperty(ref sectionBackColor, value); }
        }

        private HorizontalAlignment sectionAlignment;
        public HorizontalAlignment SectionAlignment
        {
            get { return sectionAlignment; }
            set { SetProperty(ref sectionAlignment, value); }
        }

        public IChangeTracker CompositeChangeTracker { get; private set; }

        public void Dispose()
        {
            CompositeChangeTracker.Dispose();
        }
    }
}
