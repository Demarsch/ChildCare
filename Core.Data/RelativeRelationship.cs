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
    
    public partial class RelativeRelationship
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RelativeRelationship()
        {
            this.PersonRelatives = new HashSet<PersonRelative>();
            this.RelativeRelationshipConnections = new HashSet<RelativeRelationshipConnection>();
            this.RelativeRelationshipConnections1 = new HashSet<RelativeRelationshipConnection>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<bool> MustBeMale { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonRelative> PersonRelatives { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RelativeRelationshipConnection> RelativeRelationshipConnections { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RelativeRelationshipConnection> RelativeRelationshipConnections1 { get; set; }
    }
}
