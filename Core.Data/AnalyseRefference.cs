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
    
    public partial class AnalyseRefference
    {
        public int Id { get; set; }
        public int RecordTypeId { get; set; }
        public int ParameterRecordTypeId { get; set; }
        public bool IsMale { get; set; }
        public int AgeFrom { get; set; }
        public Nullable<int> AgeTo { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public Nullable<System.DateTime> EndDateTime { get; set; }
    
        public virtual RecordType RecordType { get; set; }
        public virtual RecordType RecordType1 { get; set; }
    }
}