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
    public partial class PaymentType : ICloneable
    {
        public PaymentType()
        {
            this.RecordContracts = new HashSet<RecordContract>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Options { get; set; }
    
        [NonSerialized]
    	protected ICollection<RecordContract> recordContracts;
    
    	public virtual ICollection<RecordContract> RecordContracts
    	{
     		get { return recordContracts; }
     		set { recordContracts = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
