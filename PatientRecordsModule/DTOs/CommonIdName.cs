using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientRecordsModule.DTO
{
    public class CommonIdName
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            var CommonIdName2 = obj as CommonIdName;
            if (CommonIdName2 == null) return false;
            return this.Id == CommonIdName2.Id;
        }
    }
}
