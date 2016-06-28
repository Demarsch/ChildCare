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
    public partial class PersonIdentityDocument : ICloneable
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int IdentityDocumentTypeId { get; set; }
        public string Series { get; set; }
        public string Number { get; set; }
        public string GivenOrg { get; set; }
        public System.DateTime BeginDate { get; set; }
        public System.DateTime EndDate { get; set; }
    
        [NonSerialized]
    	protected Person person;
    
    	public virtual Person Person
    	{
     		get { return person; }
     		set { person = value; }
    	}
        [NonSerialized]
    	protected IdentityDocumentType identityDocumentType;
    
    	public virtual IdentityDocumentType IdentityDocumentType
    	{
     		get { return identityDocumentType; }
     		set { identityDocumentType = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
