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
    
    public partial class CommissionProtocol
    {
        public CommissionProtocol()
        {
            this.CommissionDecisions = new HashSet<CommissionDecision>();
        }
    
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int CommissionTypeId { get; set; }
        public Nullable<int> DecisionId { get; set; }
        public int ProtocolNumber { get; set; }
        public System.DateTime ProtocolDate { get; set; }
        public Nullable<bool> IsCompleted { get; set; }
        public bool IsExecuting { get; set; }
        public System.DateTime IncomeDateTime { get; set; }
        public Nullable<System.DateTime> BeginDateTime { get; set; }
        public Nullable<System.DateTime> CompleteDateTime { get; set; }
        public Nullable<System.DateTime> OutcomeDateTime { get; set; }
        public Nullable<System.DateTime> ToDoDateTime { get; set; }
        public string Comment { get; set; }
        public string MKB { get; set; }
        public int InUserId { get; set; }
        public int CommissionSourceId { get; set; }
        public int CommissionQuestionId { get; set; }
        public Nullable<int> PersonTalonId { get; set; }
        public Nullable<int> MedicalHelpTypeId { get; set; }
        public Nullable<int> RecordContractId { get; set; }
        public string Address { get; set; }
        public string Diagnos { get; set; }
    
        public virtual ICollection<CommissionDecision> CommissionDecisions { get; set; }
        public virtual CommissionSource CommissionSource { get; set; }
        public virtual MedicalHelpType MedicalHelpType { get; set; }
        public virtual PersonTalon PersonTalon { get; set; }
        public virtual RecordContract RecordContract { get; set; }
        public virtual User User { get; set; }
        public virtual Person Person { get; set; }
    }
}
