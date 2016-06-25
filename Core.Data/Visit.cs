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
    public partial class Visit : ICloneable
    {
        public Visit()
        {
            this.Records = new HashSet<Record>();
            this.Assignments = new HashSet<Assignment>();
        }
    
        public int Id { get; set; }
        public int VisitTemplateId { get; set; }
        public int PersonId { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public Nullable<System.DateTime> EndDateTime { get; set; }
        public int UrgentlyId { get; set; }
        public int FinancingSourceId { get; set; }
        public int ContractId { get; set; }
        public Nullable<int> SentLPUId { get; set; }
        public string OKATO { get; set; }
        public string MKB { get; set; }
        public Nullable<int> VisitResultId { get; set; }
        public Nullable<int> VisitOutcomeId { get; set; }
        public string Note { get; set; }
        public double TotalCost { get; set; }
        public Nullable<bool> IsCompleted { get; set; }
        public int ExecutionPlaceId { get; set; }
        public Nullable<int> RemovedByUserId { get; set; }
    
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
    	protected VisitOutcome visitOutcome;
    
    	public virtual VisitOutcome VisitOutcome
    	{
     		get { return visitOutcome; }
     		set { visitOutcome = value; }
    	}
        [NonSerialized]
    	protected VisitResult visitResult;
    
    	public virtual VisitResult VisitResult
    	{
     		get { return visitResult; }
     		set { visitResult = value; }
    	}
        [NonSerialized]
    	protected VisitTemplate visitTemplate;
    
    	public virtual VisitTemplate VisitTemplate
    	{
     		get { return visitTemplate; }
     		set { visitTemplate = value; }
    	}
        [NonSerialized]
    	protected Org org;
    
    	public virtual Org Org
    	{
     		get { return org; }
     		set { org = value; }
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
    	protected FinancingSource financingSource;
    
    	public virtual FinancingSource FinancingSource
    	{
     		get { return financingSource; }
     		set { financingSource = value; }
    	}
        [NonSerialized]
    	protected ExecutionPlace executionPlace;
    
    	public virtual ExecutionPlace ExecutionPlace
    	{
     		get { return executionPlace; }
     		set { executionPlace = value; }
    	}
        [NonSerialized]
    	protected ICollection<Record> records;
    
    	public virtual ICollection<Record> Records
    	{
     		get { return records; }
     		set { records = value; }
    	}
        [NonSerialized]
    	protected ICollection<Assignment> assignments;
    
    	public virtual ICollection<Assignment> Assignments
    	{
     		get { return assignments; }
     		set { assignments = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
