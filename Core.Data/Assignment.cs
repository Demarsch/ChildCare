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
    
    public partial class Assignment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Assignment()
        {
            this.Assignments1 = new HashSet<Assignment>();
            this.RecordContractItems = new HashSet<RecordContractItem>();
            this.RecordDocuments = new HashSet<RecordDocument>();
        }
    
        public int Id { get; set; }
        public Nullable<int> ParentId { get; set; }
        public int RecordTypeId { get; set; }
        public int PersonId { get; set; }
        public System.DateTime AssignDateTime { get; set; }
        public int Duration { get; set; }
        public int AssignUserId { get; set; }
        public Nullable<int> AssignLpuId { get; set; }
        public int RoomId { get; set; }
        public int FinancingSourceId { get; set; }
        public Nullable<int> CancelUserId { get; set; }
        public Nullable<System.DateTime> CancelDateTime { get; set; }
        public string Note { get; set; }
        public Nullable<int> RecordId { get; set; }
        public Nullable<int> VisitId { get; set; }
        public bool IsTemporary { get; set; }
        public System.DateTime CreationDateTime { get; set; }
        public Nullable<System.DateTime> BillingDateTime { get; set; }
        public double Cost { get; set; }
        public Nullable<int> RemovedByUserId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Assignment> Assignments1 { get; set; }
        public virtual Assignment Assignment1 { get; set; }
        public virtual FinancingSource FinancingSource { get; set; }
        public virtual Org Org { get; set; }
        public virtual Person Person { get; set; }
        public virtual Record Record { get; set; }
        public virtual Room Room { get; set; }
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
        public virtual User User2 { get; set; }
        public virtual Visit Visit { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecordContractItem> RecordContractItems { get; set; }
        public virtual RecordType RecordType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecordDocument> RecordDocuments { get; set; }
    }
}
