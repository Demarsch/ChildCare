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
    
    public partial class Org
    {
        public Org()
        {
            this.PersonSocialStatuses = new HashSet<PersonSocialStatus>();
            this.RecordContracts = new HashSet<RecordContract>();
            this.Assignments = new HashSet<Assignment>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsLpu { get; set; }
    
        public virtual ICollection<PersonSocialStatus> PersonSocialStatuses { get; set; }
        public virtual ICollection<RecordContract> RecordContracts { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }
    }
}
