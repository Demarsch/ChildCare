using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class PersonRelativeDTO
    {
        public int RelativePersonId { get; set; }

        public string ShortName { get; set; }

        public string RelativeRelationName { get; set; }

        public bool? IsRepresentative { get; set; }

        public string PhotoUri { get; set; }
    }
}
