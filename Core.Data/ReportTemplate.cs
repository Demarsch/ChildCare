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
    public partial class ReportTemplate : ICloneable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ReportTitle { get; set; }
        public string Description { get; set; }
        public bool IsDocXTemplate { get; set; }
        public string Template { get; set; }
        public bool IsAgreementDocument { get; set; }
    
    	public object Clone()
    	{
    		return MemberwiseClone();
    	}
    }
}
