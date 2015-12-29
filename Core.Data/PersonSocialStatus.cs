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
    public partial class PersonSocialStatus : ICloneable
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int SocialStatusTypeId { get; set; }
        public string Office { get; set; }
        public Nullable<int> OrgId { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
    
        [NonSerialized]
    	protected Person person;
    
    	public virtual Person Person
    	{
     		get { return person; }
     		set { person = value; }
    	}
        [NonSerialized]
    	protected SocialStatusType socialStatusType;
    
    	public virtual SocialStatusType SocialStatusType
    	{
     		get { return socialStatusType; }
     		set { socialStatusType = value; }
    	}
        [NonSerialized]
    	protected Org org;
    
    	public virtual Org Org
    	{
     		get { return org; }
     		set { org = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
