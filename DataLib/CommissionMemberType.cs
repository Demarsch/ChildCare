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
    
    public partial class CommissionMemberType
    {
        public CommissionMemberType()
        {
            this.CommissionDecisionsLinks = new HashSet<CommissionDecisionsLink>();
            this.CommissionMembers = new HashSet<CommissionMember>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public bool IsChief { get; set; }
        public bool IsSecretary { get; set; }
        public bool IsMember { get; set; }
    
        public virtual ICollection<CommissionDecisionsLink> CommissionDecisionsLinks { get; set; }
        public virtual ICollection<CommissionMember> CommissionMembers { get; set; }
    }
}