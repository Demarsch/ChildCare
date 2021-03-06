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
    public partial class RecordTypeCost : ICloneable
    {
        public int Id { get; set; }
        public int RecordTypeId { get; set; }
        public int FinancingSourceId { get; set; }
        public System.DateTime BeginDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public bool IsChild { get; set; }
        public bool IsIncome { get; set; }
        public double AccountMaterials { get; set; }
        public double Amortisation { get; set; }
        public double Salary { get; set; }
        public double SoftInventory { get; set; }
        public double Profitability { get; set; }
        public double FullPrice { get; set; }
        public System.DateTime InDateTime { get; set; }
        public string InUserLogin { get; set; }
    
        [NonSerialized]
    	protected FinancingSource financingSource;
    
    	public virtual FinancingSource FinancingSource
    	{
     		get { return financingSource; }
     		set { financingSource = value; }
    	}
        [NonSerialized]
    	protected RecordType recordType;
    
    	public virtual RecordType RecordType
    	{
     		get { return recordType; }
     		set { recordType = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
