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
    public partial class PersonRelative
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int RelativeId { get; set; }
        public int RelativeRelationshipId { get; set; }
        public bool IsRepresentative { get; set; }
    
        public virtual RelativeRelationship RelativeRelationship { get; set; }
        public virtual Person Person { get; set; }
        public virtual Person Person1 { get; set; }
    }
}
