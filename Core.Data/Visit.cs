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
    
    public partial class Visit
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Visit()
        {
            this.Assignments = new HashSet<Assignment>();
            this.Records = new HashSet<Record>();
        }
    
        public int Id { get; set; }
        public int VisitTemplateId { get; set; }
        public int PersonId { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public Nullable<System.DateTime> EndDateTime { get; set; }
        public int UrgentlyId { get; set; }
        public int FinancingSourceId { get; set; }
        public int ContractId { get; set; }
        public Nullable<int> SentLPUId { get; set; }
        public string OKATO { get; set; }
        public string MKB { get; set; }
        public Nullable<int> VisitResultId { get; set; }
        public Nullable<int> VisitOutcomeId { get; set; }
        public string Note { get; set; }
        public double TotalCost { get; set; }
        public Nullable<bool> IsCompleted { get; set; }
        public int ExecutionPlaceId { get; set; }
        public Nullable<int> RemovedByUserId { get; set; }
    
        public virtual FinancingSource FinancingSource { get; set; }
        public virtual Person Person { get; set; }
        public virtual RecordContract RecordContract { get; set; }
        public virtual User User { get; set; }
        public virtual VisitOutcome VisitOutcome { get; set; }
        public virtual VisitResult VisitResult { get; set; }
        public virtual ExecutionPlace ExecutionPlace { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Assignment> Assignments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Record> Records { get; set; }
        public virtual VisitTemplate VisitTemplate { get; set; }
        public virtual Org Org { get; set; }
        public virtual Urgently Urgently { get; set; }
    }
}
