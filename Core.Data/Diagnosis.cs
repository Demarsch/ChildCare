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
    public partial class Diagnosis : ICloneable
    {
        public int Id { get; set; }
        public int PersonDiagnosId { get; set; }
        public int DiagnosLevelId { get; set; }
        public string DiagnosText { get; set; }
        public string MKB { get; set; }
        public Nullable<int> ComplicationId { get; set; }
        public bool IsMainDiagnos { get; set; }
        public string Options { get; set; }
        public System.DateTime InDateTime { get; set; }
        public int InPersonId { get; set; }
    
        [NonSerialized]
    	protected Complication complication;
    
    	public virtual Complication Complication
    	{
     		get { return complication; }
     		set { complication = value; }
    	}
        [NonSerialized]
    	protected DiagnosLevel diagnosLevel;
    
    	public virtual DiagnosLevel DiagnosLevel
    	{
     		get { return diagnosLevel; }
     		set { diagnosLevel = value; }
    	}
        [NonSerialized]
    	protected PersonDiagnos personDiagnos;
    
    	public virtual PersonDiagnos PersonDiagnos
    	{
     		get { return personDiagnos; }
     		set { personDiagnos = value; }
    	}
        [NonSerialized]
    	protected Person person;
    
    	public virtual Person Person
    	{
     		get { return person; }
     		set { person = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
