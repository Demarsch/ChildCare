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
    
    public partial class FinacingSource
    {
        public FinacingSource()
        {
            this.RecordContracts = new HashSet<RecordContract>();
        }
    
        public int Id { get; set; }
        public int Name { get; set; }
        public int ShortName { get; set; }
    
        public virtual ICollection<RecordContract> RecordContracts { get; set; }
    }
}
