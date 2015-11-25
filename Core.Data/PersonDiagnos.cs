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
    
    public partial class PersonDiagnos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PersonDiagnos()
        {
            this.Diagnoses = new HashSet<Diagnosis>();
        }
    
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int RecordId { get; set; }
        public int DiagnosTypeId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Diagnosis> Diagnoses { get; set; }
        public virtual Person Person { get; set; }
        public virtual Record Record { get; set; }
        public virtual DiagnosType DiagnosType { get; set; }
    }
}
