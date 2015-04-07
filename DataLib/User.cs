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
    
    public partial class User
    {
        public User()
        {
            this.Assignments = new HashSet<Assignment>();
            this.Assignments1 = new HashSet<Assignment>();
            this.Records = new HashSet<Record>();
            this.UserPermissions = new HashSet<UserPermission>();
            this.CommissionProtocols = new HashSet<CommissionProtocol>();
            this.UserMessages = new HashSet<UserMessage>();
            this.UserMessages1 = new HashSet<UserMessage>();
        }
    
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string SID { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
    
        public virtual Person Person { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }
        public virtual ICollection<Assignment> Assignments1 { get; set; }
        public virtual ICollection<Record> Records { get; set; }
        public virtual ICollection<UserPermission> UserPermissions { get; set; }
        public virtual ICollection<CommissionProtocol> CommissionProtocols { get; set; }
        public virtual ICollection<UserMessage> UserMessages { get; set; }
        public virtual ICollection<UserMessage> UserMessages1 { get; set; }
    }
}
