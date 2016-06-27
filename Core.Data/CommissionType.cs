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
    public partial class CommissionType : ICloneable
    {
        public CommissionType()
        {
            this.CommissionQuestions = new HashSet<CommissionQuestion>();
            this.CommissionSources = new HashSet<CommissionSource>();
            this.CommissionMembers = new HashSet<CommissionMember>();
            this.CommissionProtocols = new HashSet<CommissionProtocol>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
    
        [NonSerialized]
    	protected ICollection<CommissionQuestion> commissionQuestions;
    
    	public virtual ICollection<CommissionQuestion> CommissionQuestions
    	{
     		get { return commissionQuestions; }
     		set { commissionQuestions = value; }
    	}
        [NonSerialized]
    	protected ICollection<CommissionSource> commissionSources;
    
    	public virtual ICollection<CommissionSource> CommissionSources
    	{
     		get { return commissionSources; }
     		set { commissionSources = value; }
    	}
        [NonSerialized]
    	protected ICollection<CommissionMember> commissionMembers;
    
    	public virtual ICollection<CommissionMember> CommissionMembers
    	{
     		get { return commissionMembers; }
     		set { commissionMembers = value; }
    	}
        [NonSerialized]
    	protected ICollection<CommissionProtocol> commissionProtocols;
    
    	public virtual ICollection<CommissionProtocol> CommissionProtocols
    	{
     		get { return commissionProtocols; }
     		set { commissionProtocols = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
