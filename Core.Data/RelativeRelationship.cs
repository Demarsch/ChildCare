
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
public partial class RelativeRelationship : ICloneable
{

    public RelativeRelationship()
    {

        this.PersonRelatives = new HashSet<PersonRelative>();

        this.RelativeRelationshipConnections = new HashSet<RelativeRelationshipConnection>();

        this.RelativeRelationshipConnections1 = new HashSet<RelativeRelationshipConnection>();

    }


    public int Id { get; set; }

    public string Name { get; set; }

    public Nullable<bool> MustBeMale { get; set; }




    [NonSerialized]
    	protected ICollection<PersonRelative> personRelatives;
    
    	public virtual ICollection<PersonRelative> PersonRelatives
    	{
     		get { return personRelatives; }
     		set { personRelatives = value; }
    	}


    [NonSerialized]
    	protected ICollection<RelativeRelationshipConnection> relativeRelationshipConnections;
    
    	public virtual ICollection<RelativeRelationshipConnection> RelativeRelationshipConnections
    	{
     		get { return relativeRelationshipConnections; }
     		set { relativeRelationshipConnections = value; }
    	}


    [NonSerialized]
    	protected ICollection<RelativeRelationshipConnection> relativeRelationshipConnections1;
    
    	public virtual ICollection<RelativeRelationshipConnection> RelativeRelationshipConnections1
    	{
     		get { return relativeRelationshipConnections1; }
     		set { relativeRelationshipConnections1 = value; }
    	}


	public object Clone()
	{
		return MemberwiseClone();
	}
}

}
