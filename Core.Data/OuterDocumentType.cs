//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Core.Data
{
    public partial class OuterDocumentType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
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
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OuterDocumentType> OuterDocumentTypes1 { get; set; }
        public virtual OuterDocumentType OuterDocumentType1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonOuterDocument> PersonOuterDocuments { get; set; }
    }
}
