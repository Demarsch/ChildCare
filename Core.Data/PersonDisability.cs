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
    public partial class PersonDisability
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int DisabilityTypeId { get; set; }
        public string Series { get; set; }
        public string Number { get; set; }
        public string GivenOrg { get; set; }
        public System.DateTime BeginDate { get; set; }
        public System.DateTime EndDate { get; set; }
    
        public virtual DisabilityType DisabilityType { get; set; }
        public virtual Person Person { get; set; }
    }
}
