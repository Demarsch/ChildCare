//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataLib
{
    using System;
    using System.Collections.Generic;
    
    public partial class SocialStatusType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SocialStatusType()
        {
            this.PersonSocialStatuses = new HashSet<PersonSocialStatus>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
        public bool NeedPlace { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonSocialStatus> PersonSocialStatuses { get; set; }
    }
}
