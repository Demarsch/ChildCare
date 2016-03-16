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
    public partial class VisitTemplate : ICloneable
    {
        public VisitTemplate()
        {
            this.Visits = new HashSet<Visit>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public Nullable<int> ContractId { get; set; }
        public Nullable<int> FinancingSourceId { get; set; }
        public Nullable<int> ExecutionPlaceId { get; set; }
        public Nullable<int> UrgentlyId { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
    
        [NonSerialized]
    	protected ExecutionPlace executionPlace;
    
    	public virtual ExecutionPlace ExecutionPlace
    	{
     		get { return executionPlace; }
     		set { executionPlace = value; }
    	}
        [NonSerialized]
    	protected RecordContract recordContract;
    
    	public virtual RecordContract RecordContract
    	{
     		get { return recordContract; }
     		set { recordContract = value; }
    	}
        [NonSerialized]
    	protected ICollection<Visit> visits;
    
    	public virtual ICollection<Visit> Visits
    	{
     		get { return visits; }
     		set { visits = value; }
    	}
        [NonSerialized]
    	protected Urgently urgently;
    
    	public virtual Urgently Urgently
    	{
     		get { return urgently; }
     		set { urgently = value; }
    	}
        [NonSerialized]
    	protected FinancingSource financingSource;
    
    	public virtual FinancingSource FinancingSource
    	{
     		get { return financingSource; }
     		set { financingSource = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
