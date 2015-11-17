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
    
    public partial class Record
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Record()
        {
            this.Assignments = new HashSet<Assignment>();
            this.Records1 = new HashSet<Record>();
            this.RecordDocuments = new HashSet<RecordDocument>();
        }
    
        public int Id { get; set; }
        public int PersonId { get; set; }
        public Nullable<int> ParentId { get; set; }
        public int VisitId { get; set; }
        public int RoomId { get; set; }
        public int RecordTypeId { get; set; }
        public int RecordPeriodId { get; set; }
        public int UrgentlyId { get; set; }
        public int BranchId { get; set; }
        public string Number { get; set; }
        public bool IsCompleted { get; set; }
        public string MKB { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
        public System.DateTime ActualDateTime { get; set; }
        public string OKATO { get; set; }
        public Nullable<System.DateTime> BillingDateTime { get; set; }
        public Nullable<int> RemovedByUserId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Assignment> Assignments { get; set; }
        public virtual Person Person { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Record> Records1 { get; set; }
        public virtual Record Record1 { get; set; }
        public virtual RecordType RecordType { get; set; }
        public virtual Room Room { get; set; }
        public virtual Urgently Urgently { get; set; }
        public virtual User User { get; set; }
        public virtual Visit Visit { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecordDocument> RecordDocuments { get; set; }
    }
}
