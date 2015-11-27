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
    
    public partial class VisitResult
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VisitResult()
        {
            this.Visits = new HashSet<Visit>();
        }
    
        public int Id { get; set; }
        public int Code { get; set; }
        public int ExecutionPlaceId { get; set; }
        public string Name { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Visit> Visits { get; set; }
        public virtual ExecutionPlace ExecutionPlace { get; set; }
    }
}
