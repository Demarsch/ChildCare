
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
public partial class User : ICloneable
{

    public User()
    {

        this.UserMessages = new HashSet<UserMessage>();

        this.UserMessages1 = new HashSet<UserMessage>();

        this.UserPermissionGroups = new HashSet<UserPermissionGroup>();

        this.Visits = new HashSet<Visit>();

        this.PersonTalons = new HashSet<PersonTalon>();

        this.PersonTalons1 = new HashSet<PersonTalon>();

        this.Records = new HashSet<Record>();

        this.CommissionDecisions = new HashSet<CommissionDecision>();

        this.CommissionDecisions1 = new HashSet<CommissionDecision>();

        this.Assignments = new HashSet<Assignment>();

        this.Assignments1 = new HashSet<Assignment>();

        this.Assignments2 = new HashSet<Assignment>();

        this.CommissionProtocols = new HashSet<CommissionProtocol>();

        this.CommissionProtocols1 = new HashSet<CommissionProtocol>();

    }


    public int Id { get; set; }

    public int PersonId { get; set; }

    public string SID { get; set; }

    public System.DateTime BeginDateTime { get; set; }

    public System.DateTime EndDateTime { get; set; }

    public string Login { get; set; }

    public string SystemName { get; set; }



    [NonSerialized]
    	protected Person person;
    
    	public virtual Person Person
    	{
     		get { return person; }
     		set { person = value; }
    	}


    [NonSerialized]
    	protected ICollection<UserMessage> userMessages;
    
    	public virtual ICollection<UserMessage> UserMessages
    	{
     		get { return userMessages; }
     		set { userMessages = value; }
    	}


    [NonSerialized]
    	protected ICollection<UserMessage> userMessages1;
    
    	public virtual ICollection<UserMessage> UserMessages1
    	{
     		get { return userMessages1; }
     		set { userMessages1 = value; }
    	}


    [NonSerialized]
    	protected ICollection<UserPermissionGroup> userPermissionGroups;
    
    	public virtual ICollection<UserPermissionGroup> UserPermissionGroups
    	{
     		get { return userPermissionGroups; }
     		set { userPermissionGroups = value; }
    	}


    [NonSerialized]
    	protected ICollection<Visit> visits;
    
    	public virtual ICollection<Visit> Visits
    	{
     		get { return visits; }
     		set { visits = value; }
    	}


    [NonSerialized]
    	protected ICollection<PersonTalon> personTalons;
    
    	public virtual ICollection<PersonTalon> PersonTalons
    	{
     		get { return personTalons; }
     		set { personTalons = value; }
    	}


    [NonSerialized]
    	protected ICollection<PersonTalon> personTalons1;
    
    	public virtual ICollection<PersonTalon> PersonTalons1
    	{
     		get { return personTalons1; }
     		set { personTalons1 = value; }
    	}


    [NonSerialized]
    	protected ICollection<Record> records;
    
    	public virtual ICollection<Record> Records
    	{
     		get { return records; }
     		set { records = value; }
    	}


    [NonSerialized]
    	protected ICollection<CommissionDecision> commissionDecisions;
    
    	public virtual ICollection<CommissionDecision> CommissionDecisions
    	{
     		get { return commissionDecisions; }
     		set { commissionDecisions = value; }
    	}


    [NonSerialized]
    	protected ICollection<CommissionDecision> commissionDecisions1;
    
    	public virtual ICollection<CommissionDecision> CommissionDecisions1
    	{
     		get { return commissionDecisions1; }
     		set { commissionDecisions1 = value; }
    	}


    [NonSerialized]
    	protected ICollection<Assignment> assignments;
    
    	public virtual ICollection<Assignment> Assignments
    	{
     		get { return assignments; }
     		set { assignments = value; }
    	}


    [NonSerialized]
    	protected ICollection<Assignment> assignments1;
    
    	public virtual ICollection<Assignment> Assignments1
    	{
     		get { return assignments1; }
     		set { assignments1 = value; }
    	}


    [NonSerialized]
    	protected ICollection<Assignment> assignments2;
    
    	public virtual ICollection<Assignment> Assignments2
    	{
     		get { return assignments2; }
     		set { assignments2 = value; }
    	}


    [NonSerialized]
    	protected ICollection<CommissionProtocol> commissionProtocols;
    
    	public virtual ICollection<CommissionProtocol> CommissionProtocols
    	{
     		get { return commissionProtocols; }
     		set { commissionProtocols = value; }
    	}


    [NonSerialized]
    	protected ICollection<CommissionProtocol> commissionProtocols1;
    
    	public virtual ICollection<CommissionProtocol> CommissionProtocols1
    	{
     		get { return commissionProtocols1; }
     		set { commissionProtocols1 = value; }
    	}


	public object Clone()
	{
		return MemberwiseClone();
	}
}

}
