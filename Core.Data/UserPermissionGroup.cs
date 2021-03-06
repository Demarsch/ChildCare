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
    public partial class UserPermissionGroup : ICloneable
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PermissionGroupId { get; set; }
    
        [NonSerialized]
    	protected PermissionGroup permissionGroup;
    
    	public virtual PermissionGroup PermissionGroup
    	{
     		get { return permissionGroup; }
     		set { permissionGroup = value; }
    	}
        [NonSerialized]
    	protected User user;
    
    	public virtual User User
    	{
     		get { return user; }
     		set { user = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
