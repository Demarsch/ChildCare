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
    
    public partial class RecordType
    {
        public RecordType()
        {
            this.Assignments = new HashSet<Assignment>();
            this.Records = new HashSet<Record>();
            this.RecordTypes1 = new HashSet<RecordType>();
            this.ScheduleItems = new HashSet<ScheduleItem>();
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
        public Nullable<int> EditorId { get; set; }
        public int RecordTypeGroupId { get; set; }
        public int MedProfileId { get; set; }
    
        public virtual ICollection<Assignment> Assignments { get; set; }
        public virtual ICollection<Record> Records { get; set; }
        public virtual ICollection<RecordType> RecordTypes1 { get; set; }
        public virtual RecordType RecordType1 { get; set; }
        public virtual ICollection<ScheduleItem> ScheduleItems { get; set; }
    }
}
