
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
public partial class EqiupmentType : ICloneable
{

    public EqiupmentType()
    {

        this.Eqiupments = new HashSet<Eqiupment>();

    }


    public int Id { get; set; }

    public string Name { get; set; }

    public int Code { get; set; }




    [NonSerialized]
    	protected ICollection<Eqiupment> eqiupments;
    
    	public virtual ICollection<Eqiupment> Eqiupments
    	{
     		get { return eqiupments; }
     		set { eqiupments = value; }
    	}


	public object Clone()
	{
		return MemberwiseClone();
	}
}

}
