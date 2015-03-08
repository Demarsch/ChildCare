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
    
    public partial class Person
    {
        public Person()
        {
            this.PersonNames = new HashSet<PersonName>();
            this.Users = new HashSet<User>();
        }
    
        public int Id { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string Snils { get; set; }
        public string MedNumber { get; set; }
        public int GenderId { get; set; }
        public Nullable<System.DateTime> DeleteDateTime { get; set; }
        public System.DateTime BirthDate { get; set; }
    
        public virtual ICollection<PersonName> PersonNames { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}