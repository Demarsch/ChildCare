//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataLib
{
    public partial class PersonAddress
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int AddressTypeId { get; set; }
        public string OkatoText { get; set; }
        public string UserText { get; set; }
        public string House { get; set; }
        public System.DateTime BeginDateTime { get; set; }
        public System.DateTime EndDateTime { get; set; }
        public string Building { get; set; }
        public string Apartment { get; set; }
    
        public virtual AddressType AddressType { get; set; }
        public virtual Person Person { get; set; }
    }
}
