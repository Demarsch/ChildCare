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
    
    public partial class DiagnosType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DiagnosType()
        {
            this.PersonDiagnoses = new HashSet<PersonDiagnos>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        public bool IsActual { get; set; }
        public bool AllowCopy { get; set; }
        public bool HasComplications { get; set; }
        public bool NeedMKB { get; set; }
        public bool NeedSetMainDiagnos { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonDiagnos> PersonDiagnoses { get; set; }
    }
}
