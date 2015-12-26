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
    
    [Serializable]
    public partial class RecordMember
    {
        public int Id { get; set; }
        public int RecordId { get; set; }
        public int PersonStaffId { get; set; }
        public int RecordTypeRolePermissionId { get; set; }
        public bool IsActive { get; set; }
    
        [NonSerialized]
    	private PersonStaff personStaff;
    
    	public virtual PersonStaff PersonStaff
    	{
     		get { return personStaff; }
     		set { personStaff = value; }
    	}
        [NonSerialized]
    	private RecordTypeRolePermission recordTypeRolePermission;
    
    	public virtual RecordTypeRolePermission RecordTypeRolePermission
    	{
     		get { return recordTypeRolePermission; }
     		set { recordTypeRolePermission = value; }
    	}
        [NonSerialized]
    	private Record record;
    
    	public virtual Record Record
    	{
     		get { return record; }
     		set { record = value; }
    	}
    }
}
