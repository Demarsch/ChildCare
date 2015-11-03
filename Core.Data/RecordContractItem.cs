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
    
    public partial class RecordContractItem
    {
        public int Id { get; set; }
        public int RecordContractId { get; set; }
        public Nullable<int> AssignmentId { get; set; }
        public int RecordTypeId { get; set; }
        public int Count { get; set; }
        public double Cost { get; set; }
        public bool IsPaid { get; set; }
        public Nullable<int> Appendix { get; set; }
        public int InUserId { get; set; }
        public System.DateTime InDateTime { get; set; }
    
        public virtual Assignment Assignment { get; set; }
        public virtual PersonStaff PersonStaff { get; set; }
        public virtual RecordType RecordType { get; set; }
        public virtual RecordContract RecordContract { get; set; }
    }
}
