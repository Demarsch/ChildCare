using Core.Misc;
using Core.Wpf.Misc;
using PatientInfoModule.Services;
using Prism.Mvvm;
using System;
using System.Drawing;
using System.Windows;

namespace PatientInfoModule.ViewModels
{
    public class ContractItemViewModel : TrackableBindableBase, IChangeTrackerMediator, IDisposable
    {
        private readonly IRecordService recordService;

        public ContractItemViewModel(IRecordService recordService)
        {
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            this.recordService = recordService;
            ChangeTracker = new ChangeTrackerEx<ContractItemViewModel>(this);
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
                    RecordCost = (recordService.GetRecordTypeCost(recordTypeId) * recordCount);    
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

        public IChangeTracker ChangeTracker { get; private set; }

        public void Dispose()
        {
            ChangeTracker.Dispose();
        }
    }
}
