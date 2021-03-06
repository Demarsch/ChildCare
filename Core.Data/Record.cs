//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Core.Data
{
    using System;
    using System.Collections.Generic;
    
    [Serializable]
    public partial class Record : ICloneable
    {
        public Record()
        {
            this.AnalyseResults = new HashSet<AnalyseResult>();
            this.DefaultProtocols = new HashSet<DefaultProtocol>();
            this.PersonDiagnoses = new HashSet<PersonDiagnos>();
            this.RecordDocuments = new HashSet<RecordDocument>();
            this.RecordEquipments = new HashSet<RecordEquipment>();
            this.RecordMembers = new HashSet<RecordMember>();
            this.Records1 = new HashSet<Record>();
            this.Assignments = new HashSet<Assignment>();
            this.Assignments1 = new HashSet<Assignment>();
        }
    
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int VisitId { get; set; }
        public Nullable<int> ParentRecordId { get; set; }
        public Nullable<int> ParentAssignmentId { get; set; }
        public int RoomId { get; set; }
        public int RecordTypeId { get; set; }
        public int RecordPeriodId { get; set; }
        public int RecordContractId { get; set; }
        public int ExecutionPlaceId { get; set; }
        public int UrgentlyId { get; set; }
        public Nullable<int> MKBId { get; set; }
        public string MKB { get; set; }
        public bool IsCompleted { get; set; }
        public int Number { get; set; }
        public string NumberType { get; set; }
        public int NumberYear { get; set; }
        public string NumberTypeYear { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public Nullable<System.DateTime> EndDateTime { get; set; }
        public System.DateTime ActualDateTime { get; set; }
        public Nullable<System.DateTime> BillingDateTime { get; set; }
        public Nullable<int> RemovedByUserId { get; set; }
    
        [NonSerialized]
    	protected ICollection<AnalyseResult> analyseResults;
    
    	public virtual ICollection<AnalyseResult> AnalyseResults
    	{
     		get { return analyseResults; }
     		set { analyseResults = value; }
    	}
        [NonSerialized]
    	protected ICollection<DefaultProtocol> defaultProtocols;
    
    	public virtual ICollection<DefaultProtocol> DefaultProtocols
    	{
     		get { return defaultProtocols; }
     		set { defaultProtocols = value; }
    	}
        [NonSerialized]
    	protected ExecutionPlace executionPlace;
    
    	public virtual ExecutionPlace ExecutionPlace
    	{
     		get { return executionPlace; }
     		set { executionPlace = value; }
    	}
        [NonSerialized]
    	protected MKB mKB1;
    
    	public virtual MKB MKB1
    	{
     		get { return mKB1; }
     		set { mKB1 = value; }
    	}
        [NonSerialized]
    	protected ICollection<PersonDiagnos> personDiagnoses;
    
    	public virtual ICollection<PersonDiagnos> PersonDiagnoses
    	{
     		get { return personDiagnoses; }
     		set { personDiagnoses = value; }
    	}
        [NonSerialized]
    	protected Person person;
    
    	public virtual Person Person
    	{
     		get { return person; }
     		set { person = value; }
    	}
        [NonSerialized]
    	protected RecordContract recordContract;
    
    	public virtual RecordContract RecordContract
    	{
     		get { return recordContract; }
     		set { recordContract = value; }
    	}
        [NonSerialized]
    	protected ICollection<RecordDocument> recordDocuments;
    
    	public virtual ICollection<RecordDocument> RecordDocuments
    	{
     		get { return recordDocuments; }
     		set { recordDocuments = value; }
    	}
        [NonSerialized]
    	protected ICollection<RecordEquipment> recordEquipments;
    
    	public virtual ICollection<RecordEquipment> RecordEquipments
    	{
     		get { return recordEquipments; }
     		set { recordEquipments = value; }
    	}
        [NonSerialized]
    	protected ICollection<RecordMember> recordMembers;
    
    	public virtual ICollection<RecordMember> RecordMembers
    	{
     		get { return recordMembers; }
     		set { recordMembers = value; }
    	}
        [NonSerialized]
    	protected RecordPeriod recordPeriod;
    
    	public virtual RecordPeriod RecordPeriod
    	{
     		get { return recordPeriod; }
     		set { recordPeriod = value; }
    	}
        [NonSerialized]
    	protected ICollection<Record> records1;
    
    	public virtual ICollection<Record> Records1
    	{
     		get { return records1; }
     		set { records1 = value; }
    	}
        [NonSerialized]
    	protected Record record1;
    
    	public virtual Record Record1
    	{
     		get { return record1; }
     		set { record1 = value; }
    	}
        [NonSerialized]
    	protected Room room;
    
    	public virtual Room Room
    	{
     		get { return room; }
     		set { room = value; }
    	}
        [NonSerialized]
    	protected Urgently urgently;
    
    	public virtual Urgently Urgently
    	{
     		get { return urgently; }
     		set { urgently = value; }
    	}
        [NonSerialized]
    	protected User user;
    
    	public virtual User User
    	{
     		get { return user; }
     		set { user = value; }
    	}
        [NonSerialized]
    	protected Visit visit;
    
    	public virtual Visit Visit
    	{
     		get { return visit; }
     		set { visit = value; }
    	}
        [NonSerialized]
    	protected RecordType recordType;
    
    	public virtual RecordType RecordType
    	{
     		get { return recordType; }
     		set { recordType = value; }
    	}
        [NonSerialized]
    	protected ICollection<Assignment> assignments;
    
    	public virtual ICollection<Assignment> Assignments
    	{
     		get { return assignments; }
     		set { assignments = value; }
    	}
        [NonSerialized]
    	protected ICollection<Assignment> assignments1;
    
    	public virtual ICollection<Assignment> Assignments1
    	{
     		get { return assignments1; }
     		set { assignments1 = value; }
    	}
        [NonSerialized]
    	protected Assignment assignment;
    
    	public virtual Assignment Assignment
    	{
     		get { return assignment; }
     		set { assignment = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
