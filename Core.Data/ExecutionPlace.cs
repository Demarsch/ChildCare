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
    public partial class ExecutionPlace : ICloneable
    {
        public ExecutionPlace()
        {
            this.RecordPeriods = new HashSet<RecordPeriod>();
            this.VisitOutcomes = new HashSet<VisitOutcome>();
            this.VisitResults = new HashSet<VisitResult>();
            this.Visits = new HashSet<Visit>();
            this.VisitTemplates = new HashSet<VisitTemplate>();
            this.Assignments = new HashSet<Assignment>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string UseName { get; set; }
        public bool IsActive { get; set; }
        public string Options { get; set; }
    
        [NonSerialized]
    	protected ICollection<RecordPeriod> recordPeriods;
    
    	public virtual ICollection<RecordPeriod> RecordPeriods
    	{
     		get { return recordPeriods; }
     		set { recordPeriods = value; }
    	}
        [NonSerialized]
    	protected ICollection<VisitOutcome> visitOutcomes;
    
    	public virtual ICollection<VisitOutcome> VisitOutcomes
    	{
     		get { return visitOutcomes; }
     		set { visitOutcomes = value; }
    	}
        [NonSerialized]
    	protected ICollection<VisitResult> visitResults;
    
    	public virtual ICollection<VisitResult> VisitResults
    	{
     		get { return visitResults; }
     		set { visitResults = value; }
    	}
        [NonSerialized]
    	protected ICollection<Visit> visits;
    
    	public virtual ICollection<Visit> Visits
    	{
     		get { return visits; }
     		set { visits = value; }
    	}
        [NonSerialized]
    	protected ICollection<VisitTemplate> visitTemplates;
    
    	public virtual ICollection<VisitTemplate> VisitTemplates
    	{
     		get { return visitTemplates; }
     		set { visitTemplates = value; }
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
