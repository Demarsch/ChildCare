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
    
    public partial class RecordTypeUnit
    {
        public int Id { get; set; }
        public int RecordTypeId { get; set; }
        public int UnitId { get; set; }
    
        public virtual RecordType RecordType { get; set; }
        public virtual Unit Unit { get; set; }
    }
}
