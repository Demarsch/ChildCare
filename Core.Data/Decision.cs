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
    public partial class Decision : ICloneable
    {
        public Decision()
        {
            this.CommissionDecisions = new HashSet<CommissionDecision>();
            this.CommissionDecisionsLinks = new HashSet<CommissionDecisionsLink>();
            this.CommissionProtocols = new HashSet<CommissionProtocol>();
            this.Decisions1 = new HashSet<Decision>();
        }
    
        public int Id { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
        public bool IsPositive { get; set; }
        public bool IsNegative { get; set; }
        public bool IsNeutral { get; set; }
    
        [NonSerialized]
    	protected ICollection<CommissionDecision> commissionDecisions;
    
    	public virtual ICollection<CommissionDecision> CommissionDecisions
    	{
     		get { return commissionDecisions; }
     		set { commissionDecisions = value; }
    	}
        [NonSerialized]
    	protected ICollection<CommissionDecisionsLink> commissionDecisionsLinks;
    
    	public virtual ICollection<CommissionDecisionsLink> CommissionDecisionsLinks
    	{
     		get { return commissionDecisionsLinks; }
     		set { commissionDecisionsLinks = value; }
    	}
        [NonSerialized]
    	protected ICollection<CommissionProtocol> commissionProtocols;
    
    	public virtual ICollection<CommissionProtocol> CommissionProtocols
    	{
     		get { return commissionProtocols; }
     		set { commissionProtocols = value; }
    	}
        [NonSerialized]
    	protected ICollection<Decision> decisions1;
    
    	public virtual ICollection<Decision> Decisions1
    	{
     		get { return decisions1; }
     		set { decisions1 = value; }
    	}
        [NonSerialized]
    	protected Decision decision1;
    
    	public virtual Decision Decision1
    	{
     		get { return decision1; }
     		set { decision1 = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
