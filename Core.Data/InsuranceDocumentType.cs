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
    public partial class InsuranceDocumentType : ICloneable
    {
        public InsuranceDocumentType()
        {
            this.InsuranceDocuments = new HashSet<InsuranceDocument>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
    
        [NonSerialized]
    	protected ICollection<InsuranceDocument> insuranceDocuments;
    
    	public virtual ICollection<InsuranceDocument> InsuranceDocuments
    	{
     		get { return insuranceDocuments; }
     		set { insuranceDocuments = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
