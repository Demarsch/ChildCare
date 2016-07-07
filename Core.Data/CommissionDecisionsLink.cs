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
    public partial class CommissionDecisionsLink : ICloneable
    {
        public int Id { get; set; }
        public int CommissionQuestionId { get; set; }
        public int CommissionTypeMemberId { get; set; }
        public int DecisionId { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
    
        [NonSerialized]
    	protected CommissionMemberType commissionMemberType;
    
    	public virtual CommissionMemberType CommissionMemberType
    	{
     		get { return commissionMemberType; }
     		set { commissionMemberType = value; }
    	}
        [NonSerialized]
    	protected Decision decision;
    
    	public virtual Decision Decision
    	{
     		get { return decision; }
     		set { decision = value; }
    	}
        [NonSerialized]
    	protected CommissionQuestion commissionQuestion;
    
    	public virtual CommissionQuestion CommissionQuestion
    	{
     		get { return commissionQuestion; }
     		set { commissionQuestion = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
