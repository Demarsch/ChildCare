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
    public partial class PersonMaritalStatus
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int MaritalStatusId { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
    
        public virtual MaritalStatus MaritalStatus { get; set; }
        public virtual Person Person { get; set; }
    }
}
