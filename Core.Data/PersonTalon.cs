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
    public partial class PersonTalon : ICloneable
    {
        public PersonTalon()
        {
            this.CommissionProtocols = new HashSet<CommissionProtocol>();
        }
    
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string TalonNumber { get; set; }
        public System.DateTime TalonDateTime { get; set; }
        public string MKB { get; set; }
        public string Comment { get; set; }
        public int RecordContractId { get; set; }
        public int MedicalHelpTypeId { get; set; }
        public int PersonAddressId { get; set; }
        public Nullable<bool> IsCompleted { get; set; }
        public int InUserId { get; set; }
        public Nullable<int> RemovedByUserId { get; set; }
    
        [NonSerialized]
    	protected MedicalHelpType medicalHelpType;
    
    	public virtual MedicalHelpType MedicalHelpType
    	{
     		get { return medicalHelpType; }
     		set { medicalHelpType = value; }
    	}
        [NonSerialized]
    	protected PersonAddress personAddress;
    
    	public virtual PersonAddress PersonAddress
    	{
     		get { return personAddress; }
     		set { personAddress = value; }
    	}
        [NonSerialized]
    	protected Person person;
    
    	public virtual Person Person
    	{
     		get { return person; }
     		set { person = value; }
    	}
        [NonSerialized]
    	protected User user;
    
    	public virtual User User
    	{
     		get { return user; }
     		set { user = value; }
    	}
        [NonSerialized]
    	protected RecordContract recordContract;
    
    	public virtual RecordContract RecordContract
    	{
     		get { return recordContract; }
     		set { recordContract = value; }
    	}
        [NonSerialized]
    	protected User user1;
    
    	public virtual User User1
    	{
     		get { return user1; }
     		set { user1 = value; }
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
