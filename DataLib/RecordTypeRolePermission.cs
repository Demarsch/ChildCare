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
    
    public partial class RecordTypeRolePermission
    {
        public int Id { get; set; }
        public int RecordTypeId { get; set; }
        public int RecordTypeMemberRoleId { get; set; }
        public int PermissionId { get; set; }
        public bool IsSign { get; set; }
        public bool IsRequired { get; set; }
    
        public virtual Permission Permission { get; set; }
        public virtual RecordTypeRole RecordTypeRole { get; set; }
        public virtual RecordType RecordType { get; set; }
    }
}