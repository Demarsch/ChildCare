
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
public partial class PersonHealthGroup : ICloneable
{

    public int Id { get; set; }

    public int PersonId { get; set; }

    public int HealthGroupId { get; set; }

    public System.DateTime BeginDateTime { get; set; }

    public System.DateTime EndDateTime { get; set; }



    [NonSerialized]
    	protected HealthGroup healthGroup;
    
    	public virtual HealthGroup HealthGroup
    	{
     		get { return healthGroup; }
     		set { healthGroup = value; }
    	}

    [NonSerialized]
    	protected Person person;
    
    	public virtual Person Person
    	{
     		get { return person; }
     		set { person = value; }
    	}


	public object Clone()
	{
		return MemberwiseClone();
	}
}

}
