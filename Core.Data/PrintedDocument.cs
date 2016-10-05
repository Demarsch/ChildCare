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
    public partial class PrintedDocument : ICloneable
    {
        public PrintedDocument()
        {
            this.CommissionQuestions = new HashSet<CommissionQuestion>();
            this.CommissionTypes = new HashSet<CommissionType>();
            this.PrintedDocuments1 = new HashSet<PrintedDocument>();
        }
    
        public int Id { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public Nullable<int> ReportTemplateId { get; set; }
        public string Options { get; set; }
        public int Priority { get; set; }
    
        [NonSerialized]
    	protected ICollection<CommissionQuestion> commissionQuestions;
    
    	public virtual ICollection<CommissionQuestion> CommissionQuestions
    	{
     		get { return commissionQuestions; }
     		set { commissionQuestions = value; }
    	}
        [NonSerialized]
    	protected ICollection<CommissionType> commissionTypes;
    
    	public virtual ICollection<CommissionType> CommissionTypes
    	{
     		get { return commissionTypes; }
     		set { commissionTypes = value; }
    	}
        [NonSerialized]
    	protected ICollection<PrintedDocument> printedDocuments1;
    
    	public virtual ICollection<PrintedDocument> PrintedDocuments1
    	{
     		get { return printedDocuments1; }
     		set { printedDocuments1 = value; }
    	}
        [NonSerialized]
    	protected PrintedDocument printedDocument1;
    
    	public virtual PrintedDocument PrintedDocument1
    	{
     		get { return printedDocument1; }
     		set { printedDocument1 = value; }
    	}
        [NonSerialized]
    	protected ReportTemplate reportTemplate;
    
    	public virtual ReportTemplate ReportTemplate
    	{
     		get { return reportTemplate; }
     		set { reportTemplate = value; }
    	}
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
