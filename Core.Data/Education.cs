
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
public partial class Education : ICloneable
{

    public Education()
    {

        this.PersonEducations = new HashSet<PersonEducation>();

    }


    public int Id { get; set; }

    public string Name { get; set; }




    [NonSerialized]
    	protected ICollection<PersonEducation> personEducations;
    
    	public virtual ICollection<PersonEducation> PersonEducations
    	{
     		get { return personEducations; }
     		set { personEducations = value; }
    	}


	public object Clone()
	{
		return MemberwiseClone();
	}
}

}
