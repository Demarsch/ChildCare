
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
public partial class RecordEquipment : ICloneable
{

    public int Id { get; set; }

    public int RecordId { get; set; }

    public int EquipmentId { get; set; }

    public Nullable<System.DateTime> BeginDateTime { get; set; }

    public Nullable<System.DateTime> EndDateTime { get; set; }

    public double Duration { get; set; }



    [NonSerialized]
    	protected Eqiupment eqiupment;
    
    	public virtual Eqiupment Eqiupment
    	{
     		get { return eqiupment; }
     		set { eqiupment = value; }
    	}

    [NonSerialized]
    	protected Record record;
    
    	public virtual Record Record
    	{
     		get { return record; }
     		set { record = value; }
    	}


	public object Clone()
	{
		return MemberwiseClone();
	}
}

}
