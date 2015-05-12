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
            this.Assignments = new HashSet<Assignment>();
            this.CommissionProtocols = new HashSet<CommissionProtocol>();
            this.InsuranceDocuments = new HashSet<InsuranceDocument>();
            this.PersonAddresses = new HashSet<PersonAddress>();
            this.PersonIdentityDocuments = new HashSet<PersonIdentityDocument>();
            this.PersonNames = new HashSet<PersonName>();
            this.PersonRelatives = new HashSet<PersonRelative>();
            this.PersonRelatives1 = new HashSet<PersonRelative>();
            this.PersonStaffs = new HashSet<PersonStaff>();
            this.PersonTalons = new HashSet<PersonTalon>();
            this.Records = new HashSet<Record>();
            this.Users = new HashSet<User>();
            this.PersonDisabilities = new HashSet<PersonDisability>();
        }
    
        public int Id { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public System.DateTime BirthDate { get; set; }
        public string Snils { get; set; }
        public string MedNumber { get; set; }
        public int GenderId { get; set; }
        public Nullable<System.DateTime> DeleteDateTime { get; set; }
    
        public virtual ICollection<Assignment> Assignments { get; set; }
        public virtual ICollection<CommissionProtocol> CommissionProtocols { get; set; }
        public virtual Gender Gender { get; set; }
        public virtual ICollection<InsuranceDocument> InsuranceDocuments { get; set; }
        public virtual ICollection<PersonAddress> PersonAddresses { get; set; }
        public virtual ICollection<PersonIdentityDocument> PersonIdentityDocuments { get; set; }
        public virtual ICollection<PersonName> PersonNames { get; set; }
        public virtual ICollection<PersonRelative> PersonRelatives { get; set; }
        public virtual ICollection<PersonRelative> PersonRelatives1 { get; set; }
        public virtual ICollection<PersonStaff> PersonStaffs { get; set; }
        public virtual ICollection<PersonTalon> PersonTalons { get; set; }
        public virtual ICollection<Record> Records { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<PersonDisability> PersonDisabilities { get; set; }
    }
}
