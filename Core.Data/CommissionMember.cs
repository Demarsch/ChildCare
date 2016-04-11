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
    public partial class CommissionMember : ICloneable
    {
        public CommissionMember()
        {
            this.CommissionDecisions = new HashSet<CommissionDecision>();
        }
    
        public int Id { get; set; }
        public Nullable<int> PersonStaffId { get; set; }
        public Nullable<int> StaffId { get; set; }
        public int CommissionMemberTypeId { get; set; }
        public int CommissionTypeId { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
    
        [NonSerialized]
    	protected ICollection<CommissionDecision> commissionDecisions;
    
    	public virtual ICollection<CommissionDecision> CommissionDecisions
    	{
     		get { return commissionDecisions; }
     		set { commissionDecisions = value; }
    	}
        [NonSerialized]
    	protected CommissionMemberType commissionMemberType;
    
    	public virtual CommissionMemberType CommissionMemberType
    	{
     		get { return commissionMemberType; }
     		set { commissionMemberType = value; }
    	}
        [NonSerialized]
    	protected CommissionType commissionType;
    
    	public virtual CommissionType CommissionType
    	{
     		get { return commissionType; }
     		set { commissionType = value; }
    	}
        [NonSerialized]
    	protected PersonStaff personStaff;
    
    	public virtual PersonStaff PersonStaff
    	{
     		get { return personStaff; }
     		set { personStaff = value; }
    	}
        [NonSerialized]
    	protected Staff staff;
    
    	public virtual Staff Staff
    	{
     		get { return staff; }
     		set { staff = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
