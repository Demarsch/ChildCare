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
    public partial class OuterDocumentType : ICloneable
    {
        public OuterDocumentType()
        {
            this.OuterDocumentTypes1 = new HashSet<OuterDocumentType>();
            this.PersonOuterDocuments = new HashSet<PersonOuterDocument>();
        }
    
        public int Id { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string Name { get; set; }
        public bool HasDate { get; set; }
        public System.DateTime BeginDate { get; set; }
        public System.DateTime EndDate { get; set; }
    
        [NonSerialized]
    	protected ICollection<OuterDocumentType> outerDocumentTypes1;
    
    	public virtual ICollection<OuterDocumentType> OuterDocumentTypes1
    	{
     		get { return outerDocumentTypes1; }
     		set { outerDocumentTypes1 = value; }
    	}
        [NonSerialized]
    	protected OuterDocumentType outerDocumentType1;
    
    	public virtual OuterDocumentType OuterDocumentType1
    	{
     		get { return outerDocumentType1; }
     		set { outerDocumentType1 = value; }
    	}
        [NonSerialized]
    	protected ICollection<PersonOuterDocument> personOuterDocuments;
    
    	public virtual ICollection<PersonOuterDocument> PersonOuterDocuments
    	{
     		get { return personOuterDocuments; }
     		set { personOuterDocuments = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
