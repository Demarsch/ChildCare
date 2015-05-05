﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ModelContext : DbContext
    {
        public ModelContext()
            : base("name=ModelContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<PersonName> PersonNames { get; set; }
        public virtual DbSet<Person> Persons { get; set; }
        public virtual DbSet<Gender> Genders { get; set; }
        public virtual DbSet<Record> Records { get; set; }
        public virtual DbSet<InsuranceCompany> InsuranceCompanies { get; set; }
        public virtual DbSet<InsuranceDocument> InsuranceDocuments { get; set; }
        public virtual DbSet<InsuranceDocumentType> InsuranceDocumentTypes { get; set; }
        public virtual DbSet<ChangeNameReason> ChangeNameReasons { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<UserPermission> UserPermissions { get; set; }
        public virtual DbSet<DBSetting> DBSettings { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<PersonRelative> PersonRelatives { get; set; }
        public virtual DbSet<RelativeRelationship> RelativeRelationships { get; set; }
        public virtual DbSet<Branch> Branches { get; set; }
        public virtual DbSet<CommissionDecision> CommissionDecisions { get; set; }
        public virtual DbSet<CommissionDecisionsLink> CommissionDecisionsLinks { get; set; }
        public virtual DbSet<CommissionMember> CommissionMembers { get; set; }
        public virtual DbSet<CommissionMemberType> CommissionMemberTypes { get; set; }
        public virtual DbSet<CommissionProtocolMember> CommissionProtocolMembers { get; set; }
        public virtual DbSet<CommissionProtocol> CommissionProtocols { get; set; }
        public virtual DbSet<CommissionType> CommissionTypes { get; set; }
        public virtual DbSet<Decision> Decisions { get; set; }
        public virtual DbSet<PersonStaff> PersonStaffs { get; set; }
        public virtual DbSet<Staff> Staffs { get; set; }
        public virtual DbSet<CommissionQuestion> CommissionQuestions { get; set; }
        public virtual DbSet<CommissionSource> CommissionSources { get; set; }
        public virtual DbSet<MedicalHelpType> MedicalHelpTypes { get; set; }
        public virtual DbSet<PersonTalon> PersonTalons { get; set; }
        public virtual DbSet<RecordContract> RecordContracts { get; set; }
        public virtual DbSet<UserMessage> UserMessages { get; set; }
        public virtual DbSet<UserMessageType> UserMessageTypes { get; set; }
        public virtual DbSet<ClosedDate> ClosedDates { get; set; }
        public virtual DbSet<AddressType> AddressTypes { get; set; }
        public virtual DbSet<Okato> Okatos { get; set; }
        public virtual DbSet<PersonAddress> PersonAddresses { get; set; }
        public virtual DbSet<ScheduleItem> ScheduleItems { get; set; }
        public virtual DbSet<RecordType> RecordTypes { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<FinacingSource> FinacingSources { get; set; }
        public virtual DbSet<Assignment> Assignments { get; set; }
        public virtual DbSet<PermissionLink> PermissionLinks { get; set; }
    }
}
