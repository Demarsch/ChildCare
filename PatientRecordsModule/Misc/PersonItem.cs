using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.PatientRecords.Misc
{
    public class PersonItem
    {
        public int Id { get; set; }
        public ItemType Type { get; set; }

        public override bool Equals(object obj)
        {
            var personItem2 = obj as PersonItem;
            if (personItem2 == null) return false;
            return this.Id == personItem2.Id && this.Type == personItem2.Type;
        }
    }

    public enum ItemType
    {
        Visit,
        Record,
        Assignment
    }
}
