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
    
    public partial class MedicalHelpType
    {
        public MedicalHelpType()
        {
            this.CommissionProtocols = new HashSet<CommissionProtocol>();
            this.PersonTalons = new HashSet<PersonTalon>();
        }
    
        public int Id { get; set; }
        public Nullable<int> RecordContractId { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
    
        public virtual ICollection<CommissionProtocol> CommissionProtocols { get; set; }
        public virtual RecordContract RecordContract { get; set; }
        public virtual ICollection<PersonTalon> PersonTalons { get; set; }
    }
}
