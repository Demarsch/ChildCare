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
    public partial class User
    {
        public User()
        {
            this.CommissionProtocols = new HashSet<CommissionProtocol>();
            this.UserMessages = new HashSet<UserMessage>();
            this.UserMessages1 = new HashSet<UserMessage>();
            this.Visits = new HashSet<Visit>();
            this.UserPermisionGroups = new HashSet<UserPermisionGroup>();
            this.Assignments = new HashSet<Assignment>();
            this.Assignments1 = new HashSet<Assignment>();
            this.Assignments2 = new HashSet<Assignment>();
            this.Records = new HashSet<Record>();
        }
    
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string SID { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public Nullable<System.DateTime> EndDateTime { get; set; }
    
        [NonSerialized]
    	private ICollection<CommissionProtocol> commissionProtocols;
    
    	public virtual ICollection<CommissionProtocol> CommissionProtocols
    	{
     		get { return commissionProtocols; }
     		set { commissionProtocols = value; }
    	}
        [NonSerialized]
    	private Person person;
    
    	public virtual Person Person
    	{
     		get { return person; }
     		set { person = value; }
    	}
        [NonSerialized]
    	private ICollection<UserMessage> userMessages;
    
    	public virtual ICollection<UserMessage> UserMessages
    	{
     		get { return userMessages; }
     		set { userMessages = value; }
    	}
        [NonSerialized]
    	private ICollection<UserMessage> userMessages1;
    
    	public virtual ICollection<UserMessage> UserMessages1
    	{
     		get { return userMessages1; }
     		set { userMessages1 = value; }
    	}
        [NonSerialized]
    	private ICollection<Visit> visits;
    
    	public virtual ICollection<Visit> Visits
    	{
     		get { return visits; }
     		set { visits = value; }
    	}
        [NonSerialized]
    	private ICollection<UserPermisionGroup> userPermisionGroups;
    
    	public virtual ICollection<UserPermisionGroup> UserPermisionGroups
    	{
     		get { return userPermisionGroups; }
     		set { userPermisionGroups = value; }
    	}
        [NonSerialized]
    	private ICollection<Assignment> assignments;
    
    	public virtual ICollection<Assignment> Assignments
    	{
     		get { return assignments; }
     		set { assignments = value; }
    	}
        [NonSerialized]
    	private ICollection<Assignment> assignments1;
    
    	public virtual ICollection<Assignment> Assignments1
    	{
     		get { return assignments1; }
     		set { assignments1 = value; }
    	}
        [NonSerialized]
    	private ICollection<Assignment> assignments2;
    
    	public virtual ICollection<Assignment> Assignments2
    	{
     		get { return assignments2; }
     		set { assignments2 = value; }
    	}
        [NonSerialized]
    	private ICollection<Record> records;
    
    	public virtual ICollection<Record> Records
    	{
     		get { return records; }
     		set { records = value; }
    	}
    }
}
