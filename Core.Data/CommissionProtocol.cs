
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
public partial class CommissionProtocol : ICloneable
{

    public CommissionProtocol()
    {

        this.CommissionDecisions = new HashSet<CommissionDecision>();

    }


    public int Id { get; set; }

    public int PersonId { get; set; }

    public int CommissionTypeId { get; set; }

    public Nullable<int> DecisionId { get; set; }

    public System.DateTime CommissionDate { get; set; }

    public int CommissionNumber { get; set; }

    public int ProtocolNumber { get; set; }

    public Nullable<bool> IsCompleted { get; set; }

    public bool IsExecuting { get; set; }

    public System.DateTime IncomeDateTime { get; set; }

    public Nullable<System.DateTime> BeginDateTime { get; set; }

    public Nullable<System.DateTime> CompleteDateTime { get; set; }

    public Nullable<System.DateTime> OutcomeDateTime { get; set; }

    public Nullable<System.DateTime> ToDoDateTime { get; set; }

    public string Comment { get; set; }

    public string MKB { get; set; }

    public int CommissionSourceId { get; set; }

    public int CommissionQuestionId { get; set; }

    public Nullable<int> PersonTalonId { get; set; }

    public Nullable<int> MedicalHelpTypeId { get; set; }

    public Nullable<int> RecordContractId { get; set; }

    public int PersonAddressId { get; set; }

    public string WaitingFor { get; set; }

    public string Diagnos { get; set; }

    public Nullable<int> SentLPUId { get; set; }

    public bool IsSended { get; set; }

    public int InUserId { get; set; }

    public Nullable<int> RemovedByUserId { get; set; }




    [NonSerialized]
    	protected ICollection<CommissionDecision> commissionDecisions;
    
    	public virtual ICollection<CommissionDecision> CommissionDecisions
    	{
     		get { return commissionDecisions; }
     		set { commissionDecisions = value; }
    	}

    [NonSerialized]
    	protected CommissionSource commissionSource;
    
    	public virtual CommissionSource CommissionSource
    	{
     		get { return commissionSource; }
     		set { commissionSource = value; }
    	}

    [NonSerialized]
    	protected User user;
    
    	public virtual User User
    	{
     		get { return user; }
     		set { user = value; }
    	}

    [NonSerialized]
    	protected Decision decision;
    
    	public virtual Decision Decision
    	{
     		get { return decision; }
     		set { decision = value; }
    	}

    [NonSerialized]
    	protected MedicalHelpType medicalHelpType;
    
    	public virtual MedicalHelpType MedicalHelpType
    	{
     		get { return medicalHelpType; }
     		set { medicalHelpType = value; }
    	}

    [NonSerialized]
    	protected Org org;
    
    	public virtual Org Org
    	{
     		get { return org; }
     		set { org = value; }
    	}

    [NonSerialized]
    	protected PersonAddress personAddress;
    
    	public virtual PersonAddress PersonAddress
    	{
     		get { return personAddress; }
     		set { personAddress = value; }
    	}

    [NonSerialized]
    	protected Person person;
    
    	public virtual Person Person
    	{
     		get { return person; }
     		set { person = value; }
    	}

    [NonSerialized]
    	protected PersonTalon personTalon;
    
    	public virtual PersonTalon PersonTalon
    	{
     		get { return personTalon; }
     		set { personTalon = value; }
    	}

    [NonSerialized]
    	protected RecordContract recordContract;
    
    	public virtual RecordContract RecordContract
    	{
     		get { return recordContract; }
     		set { recordContract = value; }
    	}

    [NonSerialized]
    	protected User user1;
    
    	public virtual User User1
    	{
     		get { return user1; }
     		set { user1 = value; }
    	}

    [NonSerialized]
    	protected CommissionQuestion commissionQuestion;
    
    	public virtual CommissionQuestion CommissionQuestion
    	{
     		get { return commissionQuestion; }
     		set { commissionQuestion = value; }
    	}

    [NonSerialized]
    	protected CommissionType commissionType;
    
    	public virtual CommissionType CommissionType
    	{
     		get { return commissionType; }
     		set { commissionType = value; }
    	}


	public object Clone()
	{
		return MemberwiseClone();
	}
}

}
