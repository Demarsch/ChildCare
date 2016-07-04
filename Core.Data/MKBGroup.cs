
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
public partial class MKBGroup : ICloneable
{

    public MKBGroup()
    {

        this.MKBs = new HashSet<MKB>();

        this.MKBGroups1 = new HashSet<MKBGroup>();

    }


    public int Id { get; set; }

    public Nullable<int> ParentId { get; set; }

    public string Name { get; set; }

    public string MKBmin { get; set; }

    public string MKBmax { get; set; }




    [NonSerialized]
    	protected ICollection<MKB> mKBs;
    
    	public virtual ICollection<MKB> MKBs
    	{
     		get { return mKBs; }
     		set { mKBs = value; }
    	}


    [NonSerialized]
    	protected ICollection<MKBGroup> mKBGroups1;
    
    	public virtual ICollection<MKBGroup> MKBGroups1
    	{
     		get { return mKBGroups1; }
     		set { mKBGroups1 = value; }
    	}

    [NonSerialized]
    	protected MKBGroup mKBGroup1;
    
    	public virtual MKBGroup MKBGroup1
    	{
     		get { return mKBGroup1; }
     		set { mKBGroup1 = value; }
    	}


	public object Clone()
	{
		return MemberwiseClone();
	}
}

}
