using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminTools.DTO
{
    class UserSystemInfoDTO
    {
        public string UserName { get; set; }

        public string SID { get; set; }

        public string PrincipalName { get; set; }

        public bool Enabled { get; set; }
        
        public ICollection<Person> Persons { get; set; }
    }
}
