//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Core.Data
{
    public partial class Person
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Person()
        {
            this.Assignments = new HashSet<Assignment>();
            this.CommissionProtocols = new HashSet<CommissionProtocol>();
            this.InsuranceDocuments = new HashSet<InsuranceDocument>();
            this.PersonAddresses = new HashSet<PersonAddress>();
            this.PersonDisabilities = new HashSet<PersonDisability>();
            this.PersonEducations = new HashSet<PersonEducation>();
            this.PersonHealthGroups = new HashSet<PersonHealthGroup>();
            this.PersonIdentityDocuments = new HashSet<PersonIdentityDocument>();
            this.PersonMaritalStatuses = new HashSet<PersonMaritalStatus>();
            this.PersonNames = new HashSet<PersonName>();
            this.PersonNationalities = new HashSet<PersonNationality>();
            this.PersonOuterDocuments = new HashSet<PersonOuterDocument>();
            this.PersonRelatives = new HashSet<PersonRelative>();
            this.PersonRelatives1 = new HashSet<PersonRelative>();
            this.PersonSocialStatuses = new HashSet<PersonSocialStatus>();
            this.PersonStaffs = new HashSet<PersonStaff>();
            this.PersonTalons = new HashSet<PersonTalon>();
            this.RecordContracts = new HashSet<RecordContract>();
            this.RecordContracts1 = new HashSet<RecordContract>();
            this.Records = new HashSet<Record>();
            this.Users = new HashSet<User>();
            this.Visits = new HashSet<Visit>();
        }
    
        public int Id { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public System.DateTime BirthDate { get; set; }
        public string Snils { get; set; }
        public string MedNumber { get; set; }
        public int GenderId { get; set; }
        public Nullable<int> PhotoId { get; set; }
        public string Phones { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> DeleteDateTime { get; set; }
        public string AmbNumberString { get; set; }
        public int AmbNumber { get; set; }
        public int Year { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Assignment> Assignments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CommissionProtocol> CommissionProtocols { get; set; }
        public virtual Document Document { get; set; }
        public virtual Gender Gender { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InsuranceDocument> InsuranceDocuments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonAddress> PersonAddresses { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonDisability> PersonDisabilities { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonEducation> PersonEducations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonHealthGroup> PersonHealthGroups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonIdentityDocument> PersonIdentityDocuments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonMaritalStatus> PersonMaritalStatuses { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonName> PersonNames { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonNationality> PersonNationalities { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonOuterDocument> PersonOuterDocuments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonRelative> PersonRelatives { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonRelative> PersonRelatives1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonSocialStatus> PersonSocialStatuses { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonStaff> PersonStaffs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonTalon> PersonTalons { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecordContract> RecordContracts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecordContract> RecordContracts1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Record> Records { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<User> Users { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Visit> Visits { get; set; }
    }
}
