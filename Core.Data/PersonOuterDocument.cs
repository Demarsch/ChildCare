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
    
    public partial class PersonOuterDocument
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int OuterDocumentTypeId { get; set; }
        public int DocumentId { get; set; }
    
        public virtual Document Document { get; set; }
        public virtual OuterDocumentType OuterDocumentType { get; set; }
        public virtual Person Person { get; set; }
    }
}
