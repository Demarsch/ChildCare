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
            this.Assignments = new HashSet<Assignment>();
            this.Records = new HashSet<Record>();
            this.InsuranceDocuments = new HashSet<InsuranceDocument>();
            this.PersonRelatives = new HashSet<PersonRelative>();
            this.PersonRelatives1 = new HashSet<PersonRelative>();
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
        public virtual Gender Gender { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }
        public virtual ICollection<Record> Records { get; set; }
        public virtual ICollection<InsuranceDocument> InsuranceDocuments { get; set; }
        public virtual ICollection<PersonRelative> PersonRelatives { get; set; }
        public virtual ICollection<PersonRelative> PersonRelatives1 { get; set; }
    }
}
