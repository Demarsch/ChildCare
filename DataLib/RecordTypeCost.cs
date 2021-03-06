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
    
    public partial class RecordTypeCost
    {
        public int Id { get; set; }
        public int RecordTypeId { get; set; }
        public int FinancingSourceId { get; set; }
        public System.DateTime BeginDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public Nullable<double> FullPrice { get; set; }
    
        public virtual FinancingSource FinancingSource { get; set; }
        public virtual RecordType RecordType { get; set; }
    }
}
