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
    
    public partial class RecordContract
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RecordContract()
        {
            this.CommissionProtocols = new HashSet<CommissionProtocol>();
            this.MedicalHelpTypes = new HashSet<MedicalHelpType>();
            this.PersonTalons = new HashSet<PersonTalon>();
            this.RecordContractItems = new HashSet<RecordContractItem>();
            this.RecordContractLimits = new HashSet<RecordContractLimit>();
            this.Visits = new HashSet<Visit>();
        }
    
        public int Id { get; set; }
        public Nullable<int> Number { get; set; }
        public string ContractName { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
        public int FinancingSourceId { get; set; }
        public Nullable<int> ClientId { get; set; }
        public Nullable<int> ConsumerId { get; set; }
        public Nullable<int> OrgId { get; set; }
        public double ContractCost { get; set; }
        public int PaymentTypeId { get; set; }
        public string TransactionNumber { get; set; }
        public string TransactionDate { get; set; }
        public int Priority { get; set; }
        public string Options { get; set; }
        public System.DateTime InDateTime { get; set; }
        public int InUserId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CommissionProtocol> CommissionProtocols { get; set; }
        public virtual FinancingSource FinancingSource { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MedicalHelpType> MedicalHelpTypes { get; set; }
        public virtual Org Org { get; set; }
        public virtual PaymentType PaymentType { get; set; }
        public virtual Person Person { get; set; }
        public virtual Person Person1 { get; set; }
        public virtual PersonStaff PersonStaff { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonTalon> PersonTalons { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecordContractItem> RecordContractItems { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecordContractLimit> RecordContractLimits { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Visit> Visits { get; set; }
    }
}
