
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
public partial class PersonDiagnos : ICloneable
{

    public PersonDiagnos()
    {

        this.Diagnoses = new HashSet<Diagnosis>();

    }


    public int Id { get; set; }

    public int PersonId { get; set; }

    public int RecordId { get; set; }

    public int DiagnosTypeId { get; set; }




    [NonSerialized]
    	protected ICollection<Diagnosis> diagnoses;
    
    	public virtual ICollection<Diagnosis> Diagnoses
    	{
     		get { return diagnoses; }
     		set { diagnoses = value; }
    	}

    [NonSerialized]
    	protected Person person;
    
    	public virtual Person Person
    	{
     		get { return person; }
     		set { person = value; }
    	}

    [NonSerialized]
    	protected DiagnosType diagnosType;
    
    	public virtual DiagnosType DiagnosType
    	{
     		get { return diagnosType; }
     		set { diagnosType = value; }
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
