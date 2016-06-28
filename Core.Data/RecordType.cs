
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
public partial class RecordType : ICloneable
{

    public RecordType()
    {

        this.AnalyseRefferences = new HashSet<AnalyseRefference>();

        this.AnalyseRefferences1 = new HashSet<AnalyseRefference>();

        this.AnalyseResults = new HashSet<AnalyseResult>();

        this.RecordContractItems = new HashSet<RecordContractItem>();

        this.RecordContractLimits = new HashSet<RecordContractLimit>();

        this.Records = new HashSet<Record>();

        this.RecordTypeCosts = new HashSet<RecordTypeCost>();

        this.RecordTypeEditors = new HashSet<RecordTypeEditor>();

        this.RecordTypeRolePermissions = new HashSet<RecordTypeRolePermission>();

        this.RecordTypes1 = new HashSet<RecordType>();

        this.RecordTypeUnits = new HashSet<RecordTypeUnit>();

        this.ScheduleItems = new HashSet<ScheduleItem>();

        this.Assignments = new HashSet<Assignment>();

    }


    public int Id { get; set; }

    public Nullable<int> ParentId { get; set; }

    public string Code { get; set; }

    public string Name { get; set; }

    public string ShortName { get; set; }

    public int MinDuration { get; set; }

    public int Duration { get; set; }

    public int DisplayOrder { get; set; }

    public Nullable<bool> Assignable { get; set; }

    public int RecordTypeGroupId { get; set; }

    public int MedProfileId { get; set; }

    public string Options { get; set; }

    public string NumberType { get; set; }

    public int Priority { get; set; }

    public bool IsRecord { get; set; }

    public bool IsConsultation { get; set; }

    public bool IsAnalyse { get; set; }

    public bool IsAnalyseParameter { get; set; }

    public bool IsBedDays { get; set; }

    public bool IsAnesthesia { get; set; }

    public bool IsDiagnostic { get; set; }

    public bool IsVaccination { get; set; }

    public bool IsProcedure { get; set; }

    public bool IsNurseCare { get; set; }

    public bool IsOperation { get; set; }

    public bool IsManualTherapy { get; set; }

    public bool IsStomatology { get; set; }




    [NonSerialized]
    	protected ICollection<AnalyseRefference> analyseRefferences;
    
    	public virtual ICollection<AnalyseRefference> AnalyseRefferences
    	{
     		get { return analyseRefferences; }
     		set { analyseRefferences = value; }
    	}


    [NonSerialized]
    	protected ICollection<AnalyseRefference> analyseRefferences1;
    
    	public virtual ICollection<AnalyseRefference> AnalyseRefferences1
    	{
     		get { return analyseRefferences1; }
     		set { analyseRefferences1 = value; }
    	}


    [NonSerialized]
    	protected ICollection<AnalyseResult> analyseResults;
    
    	public virtual ICollection<AnalyseResult> AnalyseResults
    	{
     		get { return analyseResults; }
     		set { analyseResults = value; }
    	}


    [NonSerialized]
    	protected ICollection<RecordContractItem> recordContractItems;
    
    	public virtual ICollection<RecordContractItem> RecordContractItems
    	{
     		get { return recordContractItems; }
     		set { recordContractItems = value; }
    	}


    [NonSerialized]
    	protected ICollection<RecordContractLimit> recordContractLimits;
    
    	public virtual ICollection<RecordContractLimit> RecordContractLimits
    	{
     		get { return recordContractLimits; }
     		set { recordContractLimits = value; }
    	}


    [NonSerialized]
    	protected ICollection<Record> records;
    
    	public virtual ICollection<Record> Records
    	{
     		get { return records; }
     		set { records = value; }
    	}


    [NonSerialized]
    	protected ICollection<RecordTypeCost> recordTypeCosts;
    
    	public virtual ICollection<RecordTypeCost> RecordTypeCosts
    	{
     		get { return recordTypeCosts; }
     		set { recordTypeCosts = value; }
    	}


    [NonSerialized]
    	protected ICollection<RecordTypeEditor> recordTypeEditors;
    
    	public virtual ICollection<RecordTypeEditor> RecordTypeEditors
    	{
     		get { return recordTypeEditors; }
     		set { recordTypeEditors = value; }
    	}


    [NonSerialized]
    	protected ICollection<RecordTypeRolePermission> recordTypeRolePermissions;
    
    	public virtual ICollection<RecordTypeRolePermission> RecordTypeRolePermissions
    	{
     		get { return recordTypeRolePermissions; }
     		set { recordTypeRolePermissions = value; }
    	}


    [NonSerialized]
    	protected ICollection<RecordType> recordTypes1;
    
    	public virtual ICollection<RecordType> RecordTypes1
    	{
     		get { return recordTypes1; }
     		set { recordTypes1 = value; }
    	}

    [NonSerialized]
    	protected RecordType recordType1;
    
    	public virtual RecordType RecordType1
    	{
     		get { return recordType1; }
     		set { recordType1 = value; }
    	}


    [NonSerialized]
    	protected ICollection<RecordTypeUnit> recordTypeUnits;
    
    	public virtual ICollection<RecordTypeUnit> RecordTypeUnits
    	{
     		get { return recordTypeUnits; }
     		set { recordTypeUnits = value; }
    	}


    [NonSerialized]
    	protected ICollection<ScheduleItem> scheduleItems;
    
    	public virtual ICollection<ScheduleItem> ScheduleItems
    	{
     		get { return scheduleItems; }
     		set { scheduleItems = value; }
    	}


    [NonSerialized]
    	protected ICollection<Assignment> assignments;
    
    	public virtual ICollection<Assignment> Assignments
    	{
     		get { return assignments; }
     		set { assignments = value; }
    	}


	public object Clone()
	{
		return MemberwiseClone();
	}
}

}
