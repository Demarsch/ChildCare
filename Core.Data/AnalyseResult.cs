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
    public partial class AnalyseResult : ICloneable
    {
        public int Id { get; set; }
        public int RecordId { get; set; }
        public int ParameterRecordTypeId { get; set; }
        public string Value { get; set; }
        public Nullable<double> MinRef { get; set; }
        public Nullable<double> MaxRef { get; set; }
        public bool IsNormal { get; set; }
        public bool IsAboveRef { get; set; }
        public bool IsBelowRef { get; set; }
        public string Details { get; set; }
    
        [NonSerialized]
    	protected Record record;
    
    	public virtual Record Record
    	{
     		get { return record; }
     		set { record = value; }
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
