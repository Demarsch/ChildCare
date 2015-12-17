//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Core.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class RecordType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RecordType()
        {
            this.Assignments = new HashSet<Assignment>();
            this.RecordContractItems = new HashSet<RecordContractItem>();
            this.RecordContractLimits = new HashSet<RecordContractLimit>();
            this.Records = new HashSet<Record>();
            this.RecordTypeCosts = new HashSet<RecordTypeCost>();
            this.RecordTypeEditors = new HashSet<RecordTypeEditor>();
            this.RecordTypeRolePermissions = new HashSet<RecordTypeRolePermission>();
            this.RecordTypes1 = new HashSet<RecordType>();
            this.ScheduleItems = new HashSet<ScheduleItem>();
            this.RecordTypeUnits = new HashSet<RecordTypeUnit>();
            this.AnalyseRefferences = new HashSet<AnalyseRefference>();
            this.AnalyseRefferences1 = new HashSet<AnalyseRefference>();
            this.AnalyseResults = new HashSet<AnalyseResult>();
        }
    
        public int Id { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int MinDuration { get; set; }
        public int Duration { get; set; }
        public int DisplayOrder { get; set; }
        public Nullable<bool> Assignable { get; set; }
        public int RecordTypeGroupId { get; set; }
        public int MedProfileId { get; set; }
        public bool IsRecord { get; set; }
        public bool IsAnalyse { get; set; }
        public bool IsAnalyseParameter { get; set; }
        public string Options { get; set; }
        public string NumberType { get; set; }
        public int Priority { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Assignment> Assignments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecordContractItem> RecordContractItems { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecordContractLimit> RecordContractLimits { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Record> Records { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecordTypeCost> RecordTypeCosts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecordTypeEditor> RecordTypeEditors { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecordTypeRolePermission> RecordTypeRolePermissions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecordType> RecordTypes1 { get; set; }
        public virtual RecordType RecordType1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ScheduleItem> ScheduleItems { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecordTypeUnit> RecordTypeUnits { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AnalyseRefference> AnalyseRefferences { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AnalyseRefference> AnalyseRefferences1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AnalyseResult> AnalyseResults { get; set; }
    }
}
