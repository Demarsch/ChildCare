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
    public partial class CommissionDecision : ICloneable
    {
        public int Id { get; set; }
        public int CommissionProtocolId { get; set; }
        public int CommissionMemberId { get; set; }
        public bool IsOfficial { get; set; }
        public Nullable<int> DecisionId { get; set; }
        public Nullable<System.DateTime> ToDoDateTime { get; set; }
        public string Comment { get; set; }
        public int CommissionStage { get; set; }
        public bool NeedAlllMemmbersInStage { get; set; }
        public int InitiatorMemberId { get; set; }
        public Nullable<System.DateTime> DecisionDateTime { get; set; }
        public Nullable<int> RemovedByUserId { get; set; }
    
        [NonSerialized]
    	protected CommissionMember commissionMember;
    
    	public virtual CommissionMember CommissionMember
    	{
     		get { return commissionMember; }
     		set { commissionMember = value; }
    	}
        [NonSerialized]
    	protected CommissionMember commissionMember1;
    
    	public virtual CommissionMember CommissionMember1
    	{
     		get { return commissionMember1; }
     		set { commissionMember1 = value; }
    	}
        [NonSerialized]
    	protected CommissionProtocol commissionProtocol;
    
    	public virtual CommissionProtocol CommissionProtocol
    	{
     		get { return commissionProtocol; }
     		set { commissionProtocol = value; }
    	}
        [NonSerialized]
    	protected Decision decision;
    
    	public virtual Decision Decision
    	{
     		get { return decision; }
     		set { decision = value; }
    	}
        [NonSerialized]
    	protected User user;
    
    	public virtual User User
    	{
     		get { return user; }
     		set { user = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
