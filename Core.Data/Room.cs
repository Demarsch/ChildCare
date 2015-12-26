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
    public partial class Room
    {
        public Room()
        {
            this.ScheduleItems = new HashSet<ScheduleItem>();
            this.Eqiupments = new HashSet<Eqiupment>();
            this.Assignments = new HashSet<Assignment>();
            this.Records = new HashSet<Record>();
        }
    
        public int Id { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
        public string Options { get; set; }
    
        [NonSerialized]
    	private ICollection<ScheduleItem> scheduleItems;
    
    	public virtual ICollection<ScheduleItem> ScheduleItems
    	{
     		get { return scheduleItems; }
     		set { scheduleItems = value; }
    	}
        [NonSerialized]
    	private ICollection<Eqiupment> eqiupments;
    
    	public virtual ICollection<Eqiupment> Eqiupments
    	{
     		get { return eqiupments; }
     		set { eqiupments = value; }
    	}
        [NonSerialized]
    	private ICollection<Assignment> assignments;
    
    	public virtual ICollection<Assignment> Assignments
    	{
     		get { return assignments; }
     		set { assignments = value; }
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
