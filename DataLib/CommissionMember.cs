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
    
    public partial class CommissionMember
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CommissionMember()
        {
            this.CommissionDecisions = new HashSet<CommissionDecision>();
            this.CommissionDecisions1 = new HashSet<CommissionDecision>();
        }
    
        public int Id { get; set; }
        public int PersonStaffId { get; set; }
        public int CommissionMemberTypeId { get; set; }
        public int CommissionTypeId { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CommissionDecision> CommissionDecisions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CommissionDecision> CommissionDecisions1 { get; set; }
        public virtual CommissionMemberType CommissionMemberType { get; set; }
        public virtual CommissionType CommissionType { get; set; }
        public virtual PersonStaff PersonStaff { get; set; }
    }
}
