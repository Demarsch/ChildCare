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
    
    public partial class InsuranceCompany
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public InsuranceCompany()
        {
            this.InsuranceDocuments = new HashSet<InsuranceDocument>();
        }
    
        public int Id { get; set; }
        public string AddressF { get; set; }
        public string AddressJ { get; set; }
        public System.DateTime DateBegin { get; set; }
        public System.DateTime DateEdit { get; set; }
        public System.DateTime DateEnd { get; set; }
        public System.DateTime DateStart { get; set; }
        public string IndexF { get; set; }
        public string IndexJ { get; set; }
        public string INN { get; set; }
        public string KPP { get; set; }
        public string DocNumber { get; set; }
        public string NameSMOK { get; set; }
        public string NameSMOP { get; set; }
        public string OGRN { get; set; }
        public string SmoCode { get; set; }
        public string OKATO { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InsuranceDocument> InsuranceDocuments { get; set; }
    }
}
