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
    
    public partial class ChangeNameReason
    {
        public ChangeNameReason()
        {
            this.PersonNames = new HashSet<PersonName>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public bool NeedCreateNewPersonName { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
    
        public virtual ICollection<PersonName> PersonNames { get; set; }
    }
}