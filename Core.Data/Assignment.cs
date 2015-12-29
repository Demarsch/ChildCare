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
    public partial class Assignment : ICloneable
    {
        public Assignment()
        {
            this.Assignments1 = new HashSet<Assignment>();
            this.RecordContractItems = new HashSet<RecordContractItem>();
            this.RecordDocuments = new HashSet<RecordDocument>();
            this.Records = new HashSet<Record>();
        }
    
        public int Id { get; set; }
        public Nullable<int> ParentAssignmentId { get; set; }
        public Nullable<int> ParentRecordId { get; set; }
        public int RecordTypeId { get; set; }
        public int PersonId { get; set; }
        public System.DateTime AssignDateTime { get; set; }
        public int Duration { get; set; }
        public int AssignUserId { get; set; }
        public Nullable<int> AssignLpuId { get; set; }
        public int RoomId { get; set; }
        public int FinancingSourceId { get; set; }
        public int UrgentlyId { get; set; }
        public int ExecutionPlaceId { get; set; }
        public string ParametersOptions { get; set; }
        public Nullable<int> CancelUserId { get; set; }
        public Nullable<System.DateTime> CancelDateTime { get; set; }
        public string Note { get; set; }
        public Nullable<int> RecordId { get; set; }
        public Nullable<int> VisitId { get; set; }
        public bool IsTemporary { get; set; }
        public System.DateTime CreationDateTime { get; set; }
        public Nullable<System.DateTime> BillingDateTime { get; set; }
        public double Cost { get; set; }
        public Nullable<int> RemovedByUserId { get; set; }
    
        [NonSerialized]
    	protected ICollection<Assignment> assignments1;
    
    	public virtual ICollection<Assignment> Assignments1
    	{
     		get { return assignments1; }
     		set { assignments1 = value; }
    	}
        [NonSerialized]
    	protected Assignment assignment1;
    
    	public virtual Assignment Assignment1
    	{
     		get { return assignment1; }
     		set { assignment1 = value; }
    	}
        [NonSerialized]
    	protected ExecutionPlace executionPlace;
    
    	public virtual ExecutionPlace ExecutionPlace
    	{
     		get { return executionPlace; }
     		set { executionPlace = value; }
    	}
        [NonSerialized]
    	protected FinancingSource financingSource;
    
    	public virtual FinancingSource FinancingSource
    	{
     		get { return financingSource; }
     		set { financingSource = value; }
    	}
        [NonSerialized]
    	protected Org org;
    
    	public virtual Org Org
    	{
     		get { return org; }
     		set { org = value; }
    	}
        [NonSerialized]
    	protected Person person;
    
    	public virtual Person Person
    	{
     		get { return person; }
     		set { person = value; }
    	}
        [NonSerialized]
    	protected Record record;
    
    	public virtual Record Record
    	{
     		get { return record; }
     		set { record = value; }
    	}
        [NonSerialized]
    	protected Record record1;
    
    	public virtual Record Record1
    	{
     		get { return record1; }
     		set { record1 = value; }
    	}
        [NonSerialized]
    	protected RecordType recordType;
    
    	public virtual RecordType RecordType
    	{
     		get { return recordType; }
     		set { recordType = value; }
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
    	protected User user1;
    
    	public virtual User User1
    	{
     		get { return user1; }
     		set { user1 = value; }
    	}
        [NonSerialized]
    	protected User user2;
    
    	public virtual User User2
    	{
     		get { return user2; }
     		set { user2 = value; }
    	}
        [NonSerialized]
    	protected Visit visit;
    
    	public virtual Visit Visit
    	{
     		get { return visit; }
     		set { visit = value; }
    	}
        [NonSerialized]
    	protected ICollection<RecordContractItem> recordContractItems;
    
    	public virtual ICollection<RecordContractItem> RecordContractItems
    	{
     		get { return recordContractItems; }
     		set { recordContractItems = value; }
    	}
        [NonSerialized]
    	protected ICollection<RecordDocument> recordDocuments;
    
    	public virtual ICollection<RecordDocument> RecordDocuments
    	{
     		get { return recordDocuments; }
     		set { recordDocuments = value; }
    	}
        [NonSerialized]
    	protected ICollection<Record> records;
    
    	public virtual ICollection<Record> Records
    	{
     		get { return records; }
     		set { records = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
