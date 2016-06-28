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
    public partial class Urgently : ICloneable
    {
        public Urgently()
        {
            this.Visits = new HashSet<Visit>();
            this.VisitTemplates = new HashSet<VisitTemplate>();
            this.Records = new HashSet<Record>();
            this.Assignments = new HashSet<Assignment>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string PrintName { get; set; }
        public bool IsDefault { get; set; }
        public bool IsUrgently { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
    
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
