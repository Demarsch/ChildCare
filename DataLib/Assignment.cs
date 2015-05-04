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
    
    public partial class Assignment
    {
        public int Id { get; set; }
        public int RecordTypeId { get; set; }
        public int PersonId { get; set; }
        public System.DateTime AssignDateTime { get; set; }
        public int AssignUserId { get; set; }
        public int RoomId { get; set; }
        public int FinancingSourceId { get; set; }
        public Nullable<int> CancelUserId { get; set; }
        public string Note { get; set; }
        public Nullable<int> RecordId { get; set; }
        public bool IsTemporary { get; set; }
        public System.DateTime CreationDateTime { get; set; }
        public Nullable<System.DateTime> CancelDateTime { get; set; }
    
        public virtual FinacingSource FinacingSource { get; set; }
        public virtual Person Person { get; set; }
        public virtual Record Record { get; set; }
        public virtual RecordType RecordType { get; set; }
        public virtual Room Room { get; set; }
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
    }
}
